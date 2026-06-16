using LaBikeLifeDotNet.Models;

namespace LaBikeLifeDotNet.ViewModels;

public class GarageViewModel
{
    public IReadOnlyList<GarageItemViewModel> Items { get; set; } = [];
}

// une moto avec son entretien calculé et son historique
public class GarageItemViewModel
{
    public UserMotorcycle Motorcycle { get; set; } = default!;
    public IReadOnlyList<MaintenanceItemViewModel> Maintenance { get; set; } = [];
    public IReadOnlyList<MaintenanceHistoryEntryViewModel> History { get; set; } = [];
}

// une ligne de l'historique des entretiens
public class MaintenanceHistoryEntryViewModel
{
    public int RecordId { get; set; }
    public string TaskName { get; set; } = default!;
    public int Km { get; set; }
    public DateTime DateUtc { get; set; }
}
