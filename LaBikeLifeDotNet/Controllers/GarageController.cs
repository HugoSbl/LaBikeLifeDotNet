using LaBikeLifeDotNet.Data;
using LaBikeLifeDotNet.Models;
using LaBikeLifeDotNet.Services;
using LaBikeLifeDotNet.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaBikeLifeDotNet.Controllers;

[Authorize]
public class GarageController(
    ApplicationDbContext db,
    INhtsaVpicService vpic,
    IWikipediaImageService wiki,
    IMaintenanceService maintenance,
    UserManager<IdentityUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = userManager.GetUserId(User)!;
        var motos = await db.UserMotorcycles
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedUtc)
            .ToListAsync();

        var tasks = await db.MaintenanceTasks.AsNoTracking().ToListAsync();
        var taskNames = tasks.ToDictionary(t => t.Id, t => t.Name);

        var motoIds = motos.Select(m => m.Id).ToList();
        var recordsByMoto = (await db.MaintenanceRecords.AsNoTracking()
                .Where(r => motoIds.Contains(r.UserMotorcycleId))
                .ToListAsync())
            .GroupBy(r => r.UserMotorcycleId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var items = new List<GarageItemViewModel>();
        foreach (var m in motos)
        {
            var recs = recordsByMoto.TryGetValue(m.Id, out var rr) ? rr : [];

            // pour chaque entretien on prend celui fait au km le plus haut, c'est lui qui sert de référence
            var latestByTask = recs
                .GroupBy(r => r.MaintenanceTaskId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(r => r.LastDoneKm).ThenByDescending(r => r.LastDoneUtc).First());

            var history = recs
                .OrderByDescending(r => r.LastDoneUtc)
                .Select(r => new MaintenanceHistoryEntryViewModel
                {
                    RecordId = r.Id,
                    TaskName = taskNames.TryGetValue(r.MaintenanceTaskId, out var n) ? n : "Entretien",
                    Km = r.LastDoneKm,
                    DateUtc = r.LastDoneUtc
                })
                .ToList();

            items.Add(new GarageItemViewModel
            {
                Motorcycle = m,
                Maintenance = maintenance.ComputeDue(m, tasks, latestByTask),
                History = history
            });
        }

        return View(new GarageViewModel { Items = items });
    }

    // appelé en AJAX pour remplir la liste des modèles une fois la marque et l'année choisies
    [HttpGet]
    public async Task<IActionResult> Models(string make, int year)
    {
        var models = await vpic.GetModelsAsync(make, year);
        return Json(models);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToGarage(string make, int year, string model, string? vin, int mileageKm)
    {
        if (string.IsNullOrWhiteSpace(make) || string.IsNullOrWhiteSpace(model) || year < 1980)
        {
            TempData["Error"] = "Merci de choisir une marque, une année et un modèle.";
            return RedirectToAction("Index", "Home");
        }

        var moto = new UserMotorcycle
        {
            UserId = userManager.GetUserId(User)!,
            Make = make.Trim(),
            Model = model.Trim(),
            Year = year,
            Vin = string.IsNullOrWhiteSpace(vin) ? null : vin.Trim(),
            CurrentMileageKm = Math.Max(0, mileageKm),
            MileageUpdatedUtc = DateTime.UtcNow,
            CreatedUtc = DateTime.UtcNow
        };

        // si l'utilisateur a mis un VIN, on récupère les specs avec
        if (moto.Vin is not null)
        {
            var specs = await vpic.DecodeVinAsync(moto.Vin);
            if (specs is not null)
            {
                moto.DisplacementCc = specs.DisplacementCc;
                moto.Cylinders = specs.Cylinders;
                moto.FuelType = specs.FuelType;
                moto.BrakeSystem = specs.BrakeSystem;
                moto.BodyClass = specs.BodyClass;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Enrichissement visuel : résolution asynchrone d'une illustration représentative
        // du modèle au moyen du service d'imagerie Wikipedia. La requête agrège la marque
        // et le modèle, complétés du qualificatif de domaine « motorcycle » afin de
        // maximiser la pertinence sémantique du candidat retourné. En l'absence de
        // correspondance exploitable, la propriété demeure nulle et la couche de
        // présentation applique un repli gracieux (espace réservé).
        // ─────────────────────────────────────────────────────────────────────────────
        moto.ImageUrl = await wiki.GetImageUrlAsync($"{moto.Make} {moto.Model} motorcycle");

        db.UserMotorcycles.Add(moto);
        await db.SaveChangesAsync();
        TempData["Success"] = $"{moto.Make} {moto.Model} ({moto.Year}) ajoutée à votre garage.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateMileage(int id, int mileageKm)
    {
        var userId = userManager.GetUserId(User)!;
        var moto = await db.UserMotorcycles.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        if (moto is null) return NotFound();

        moto.CurrentMileageKm = Math.Max(0, mileageKm);
        moto.MileageUpdatedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Ré-exécute la procédure de résolution d'illustration pour une moto déjà persistée.
    /// </summary>
    /// <remarks>
    /// Pertinent lorsque l'illustration n'a pu être déterminée lors de l'ajout initial — par
    /// exemple en cas d'indisponibilité momentanée de l'API Wikipedia ou d'absence, à cet instant,
    /// d'un article suffisamment pertinent. L'opération est idempotente : elle écrase
    /// inconditionnellement la valeur antérieure par le meilleur candidat disponible au moment de
    /// l'invocation, ou par <c>null</c> à défaut.
    /// </remarks>
    /// <param name="id">Identifiant de la moto ciblée, nécessairement détenue par l'utilisateur courant.</param>
    /// <returns>Une redirection vers le tableau de bord du garage.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefreshPhoto(int id)
    {
        var userId = userManager.GetUserId(User)!;
        var moto = await db.UserMotorcycles.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        if (moto is null) return NotFound();

        moto.ImageUrl = await wiki.GetImageUrlAsync($"{moto.Make} {moto.Model} motorcycle");
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        var userId = userManager.GetUserId(User)!;
        var moto = await db.UserMotorcycles.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        if (moto is not null)
        {
            db.UserMotorcycles.Remove(moto);
            await db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // on note qu'un entretien a été fait, au km indiqué (sinon le km actuel)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkDone(int motorcycleId, int taskId, int doneAtKm)
    {
        var userId = userManager.GetUserId(User)!;
        var owns = await db.UserMotorcycles.AnyAsync(m => m.Id == motorcycleId && m.UserId == userId);
        var taskExists = await db.MaintenanceTasks.AnyAsync(t => t.Id == taskId);
        if (!owns || !taskExists) return NotFound();

        db.MaintenanceRecords.Add(new MaintenanceRecord
        {
            UserMotorcycleId = motorcycleId,
            MaintenanceTaskId = taskId,
            LastDoneKm = Math.Max(0, doneAtKm),
            LastDoneUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // supprime une ligne de l'historique
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRecord(int recordId)
    {
        var userId = userManager.GetUserId(User)!;
        var rec = await db.MaintenanceRecords.FirstOrDefaultAsync(r => r.Id == recordId);
        if (rec is null) return RedirectToAction(nameof(Index));

        var owns = await db.UserMotorcycles.AnyAsync(m => m.Id == rec.UserMotorcycleId && m.UserId == userId);
        if (!owns) return NotFound();

        db.MaintenanceRecords.Remove(rec);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
