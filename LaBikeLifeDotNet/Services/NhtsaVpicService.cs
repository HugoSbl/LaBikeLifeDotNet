using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using LaBikeLifeDotNet.Services.Vpic;

namespace LaBikeLifeDotNet.Services;

// les specs d'une moto, version simplifiée
public record MotorcycleSpecs(
    string? Make,
    string? Model,
    int? Year,
    int? DisplacementCc,
    int? Cylinders,
    string? FuelType,
    string? BrakeSystem,
    string? BodyClass);

public interface INhtsaVpicService
{
    Task<IReadOnlyList<string>> GetMotorcycleMakesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetModelsAsync(string make, int year, CancellationToken ct = default);
    Task<MotorcycleSpecs?> DecodeVinAsync(string vin, CancellationToken ct = default);
}

// client de l'API vPIC (gratuite, sans clé) : marques, modèles et specs via le VIN
public class NhtsaVpicService(HttpClient http, ILogger<NhtsaVpicService> logger) : INhtsaVpicService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<string>> GetMotorcycleMakesAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await http.GetFromJsonAsync<VpicListResponse<VpicMake>>(
                "vehicles/GetMakesForVehicleType/motorcycle?format=json", JsonOpts, ct);
            return Clean(resp?.Results.Select(m => m.MakeName));
        }
        catch (Exception ex)
        {
            // si l'API tombe, on renvoie vide plutôt que de planter la page
            logger.LogWarning(ex, "Échec de récupération des marques motos vPIC");
            return [];
        }
    }

    public async Task<IReadOnlyList<string>> GetModelsAsync(string make, int year, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(make)) return [];
        try
        {
            var url = $"vehicles/GetModelsForMakeYear/make/{Uri.EscapeDataString(make)}" +
                      $"/modelyear/{year}/vehicleType/motorcycle?format=json";
            var resp = await http.GetFromJsonAsync<VpicListResponse<VpicModel>>(url, JsonOpts, ct);
            return Clean(resp?.Results.Select(m => m.Model_Name));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Échec de récupération des modèles vPIC pour {Make} {Year}", make, year);
            return [];
        }
    }

    public async Task<MotorcycleSpecs?> DecodeVinAsync(string vin, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(vin)) return null;
        try
        {
            var resp = await http.GetFromJsonAsync<VpicListResponse<VpicVinValues>>(
                $"vehicles/DecodeVinValues/{Uri.EscapeDataString(vin.Trim())}?format=json", JsonOpts, ct);
            var r = resp?.Results.FirstOrDefault();
            if (r is null) return null;
            return new MotorcycleSpecs(
                Make: NullIfEmpty(r.Make),
                Model: NullIfEmpty(r.Model),
                Year: ParseInt(r.ModelYear),
                DisplacementCc: ParseRoundedInt(r.DisplacementCC),
                Cylinders: ParseInt(r.EngineCylinders),
                FuelType: NullIfEmpty(r.FuelTypePrimary),
                BrakeSystem: NullIfEmpty(r.BrakeSystemType),
                BodyClass: NullIfEmpty(r.BodyClass));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Échec du décodage VIN vPIC pour {Vin}", vin);
            return null;
        }
    }

    // on enlève les vides et les doublons, puis on trie
    private static IReadOnlyList<string> Clean(IEnumerable<string>? values) =>
        values?
            .Select(v => v?.Trim() ?? "")
            .Where(v => v.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    private static int? ParseInt(string? s) => int.TryParse(s, out var v) ? v : null;

    // arondit cylindrée
    private static int? ParseRoundedInt(string? s) =>
        double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? (int)Math.Round(v) : null;
}
