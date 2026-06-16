using System.ComponentModel.DataAnnotations;

namespace LaBikeLifeDotNet.Models;

// un type d'entretien et son intervalle classique (la liste est pré-remplie en base au démarrage)
public class MaintenanceTask
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(50)]
    public string Category { get; set; } = "Général";

    // en km (null si c'est plutôt une question de temps)
    public int? IntervalKm { get; set; }

    // en mois (genre liquide de frein tous les 2 ans)
    public int? IntervalMonths { get; set; }

    [MaxLength(300)]
    public string? Notes { get; set; }
}
