namespace LaBikeLifeDotNet.ViewModels;

// l'état d'un entretien pour une moto donnée
public enum MaintenanceStatus
{
    Overdue,  // dépassé, à faire
    DueSoon,  // ça arrive bientôt
    Ok,       // encore large
    Calendar  // au temps, mais on connaît pas la date du dernier entretien
}

// une ligne du tableau d'entretien (ce qu'on montre dans la vue)
public class MaintenanceItemViewModel
{
    public int TaskId { get; set; }
    public string Name { get; set; } = default!;
    public string Category { get; set; } = "Général";
    public string? Notes { get; set; }

    public int? IntervalKm { get; set; }
    public int? IntervalMonths { get; set; }

    // dernier entretien fait (null = jamais)
    public int? LastDoneKm { get; set; }
    public DateTime? LastDoneUtc { get; set; }

    public int? NextDueKm { get; set; }
    public int? KmRemaining { get; set; }
    public int? KmOverdue { get; set; }
    public DateTime? NextDueUtc { get; set; }

    public MaintenanceStatus Status { get; set; }
}
