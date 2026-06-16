using LaBikeLifeDotNet.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LaBikeLifeDotNet.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<UserMotorcycle> UserMotorcycles => Set<UserMotorcycle>();
    public DbSet<MaintenanceTask> MaintenanceTasks => Set<MaintenanceTask>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserMotorcycle>()
            .HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<MaintenanceRecord>(e =>
        {
            e.HasOne(r => r.UserMotorcycle)
                .WithMany()
                .HasForeignKey(r => r.UserMotorcycleId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.MaintenanceTask)
                .WithMany()
                .HasForeignKey(r => r.MaintenanceTaskId)
                .OnDelete(DeleteBehavior.Cascade);
            // c'est un historique : on peut avoir plusieurs lignes par moto/tâche, donc l'index n'est pas unique
            e.HasIndex(r => new { r.UserMotorcycleId, r.MaintenanceTaskId });
        });

        // on pré-remplit la liste des entretiens avec des intervalles classiques de moto
        builder.Entity<MaintenanceTask>().HasData(
            new MaintenanceTask { Id = 1, Name = "Vidange huile + filtre", Category = "Moteur", IntervalKm = 6000, IntervalMonths = 12, Notes = "Plus souvent en usage intensif ou huile minérale." },
            new MaintenanceTask { Id = 2, Name = "Contrôle/réglage des soupapes", Category = "Moteur", IntervalKm = 12000, Notes = "Varie selon le modèle (sportives plus fréquent)." },
            new MaintenanceTask { Id = 3, Name = "Nettoyage/graissage de la chaîne", Category = "Transmission", IntervalKm = 1000, Notes = "Et après chaque sortie sous la pluie." },
            new MaintenanceTask { Id = 4, Name = "Contrôle des plaquettes de frein", Category = "Freinage", IntervalKm = 10000, Notes = "Remplacer si épaisseur < 2-3 mm." },
            new MaintenanceTask { Id = 5, Name = "Remplacement du liquide de frein", Category = "Freinage", IntervalMonths = 24, Notes = "Tous les 2 ans, indépendamment du kilométrage." },
            new MaintenanceTask { Id = 6, Name = "Liquide de refroidissement", Category = "Moteur", IntervalKm = 24000, IntervalMonths = 24, Notes = "Motos à refroidissement liquide." },
            new MaintenanceTask { Id = 7, Name = "Bougies d'allumage", Category = "Moteur", IntervalKm = 12000, Notes = "Selon le type (iridium plus durable)." },
            new MaintenanceTask { Id = 8, Name = "Filtre à air", Category = "Moteur", IntervalKm = 12000, Notes = "Plus souvent en milieu poussiéreux." },
            new MaintenanceTask { Id = 9, Name = "Contrôle d'usure des pneus", Category = "Pneumatiques", IntervalKm = 5000, Notes = "Vérifier aussi la pression régulièrement." },
            new MaintenanceTask { Id = 10, Name = "Inspection générale", Category = "Général", IntervalKm = 6000, IntervalMonths = 12, Notes = "Visserie, câbles, niveaux, éclairage." }
        );
    }
}
