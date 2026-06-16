namespace LaBikeLifeDotNet.Models;

// une intervention faite par l'utilisateur ; on garde tout l'historique, la plus avancée sert de référence
public class MaintenanceRecord
{
    public int Id { get; set; }

    public int UserMotorcycleId { get; set; }
    public UserMotorcycle? UserMotorcycle { get; set; }

    public int MaintenanceTaskId { get; set; }
    public MaintenanceTask? MaintenanceTask { get; set; }

    public int LastDoneKm { get; set; }

    public DateTime LastDoneUtc { get; set; }
}
