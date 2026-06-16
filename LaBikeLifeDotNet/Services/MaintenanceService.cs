using LaBikeLifeDotNet.Models;
using LaBikeLifeDotNet.ViewModels;

namespace LaBikeLifeDotNet.Services;

public interface IMaintenanceService
{
    IReadOnlyList<MaintenanceItemViewModel> ComputeDue(
        UserMotorcycle moto,
        IEnumerable<MaintenanceTask> tasks,
        IReadOnlyDictionary<int, MaintenanceRecord> recordsByTaskId);
}

// calcule, pour une moto et son km, ce qui est à faire ou en retard (pas d'API pour ça)
public class MaintenanceService : IMaintenanceService
{
    // sous 500 km avant l'échéance, on considère que c'est "bientôt"
    private const int DueSoonThresholdKm = 500;

    public IReadOnlyList<MaintenanceItemViewModel> ComputeDue(
        UserMotorcycle moto,
        IEnumerable<MaintenanceTask> tasks,
        IReadOnlyDictionary<int, MaintenanceRecord> recordsByTaskId)
    {
        var km = moto.CurrentMileageKm;
        var now = DateTime.UtcNow;
        var items = new List<MaintenanceItemViewModel>();

        foreach (var t in tasks)
        {
            recordsByTaskId.TryGetValue(t.Id, out var rec);
            int? lastDoneKm = rec?.LastDoneKm;
            DateTime? lastDoneUtc = rec?.LastDoneUtc;

            int? nextDueKm = null, kmRemaining = null, kmOverdue = null;
            DateTime? nextDueUtc = null;
            var overdue = false;
            var hasKm = t.IntervalKm is > 0;

            // cas du km : prochaine fois = dernière fois (ou 0 si jamais fait) + intervalle
            if (hasKm)
            {
                var interval = t.IntervalKm!.Value;
                var baseline = lastDoneKm ?? 0;
                nextDueKm = baseline + interval;
                if (km >= nextDueKm.Value)
                {
                    overdue = true;
                    kmOverdue = km - nextDueKm.Value;
                }
                else
                {
                    kmRemaining = nextDueKm.Value - km;
                }
            }

            // cas du temps : seulement si on connaît la date du dernier entretien
            if (t.IntervalMonths is > 0 && lastDoneUtc is { } ld)
            {
                nextDueUtc = ld.AddMonths(t.IntervalMonths!.Value);
                if (now > nextDueUtc.Value) overdue = true;
            }

            MaintenanceStatus status;
            if (overdue)
                status = MaintenanceStatus.Overdue;
            else if (hasKm && kmRemaining is <= DueSoonThresholdKm)
                status = MaintenanceStatus.DueSoon;
            else if (hasKm || nextDueUtc is not null)
                status = MaintenanceStatus.Ok;
            else
                status = MaintenanceStatus.Calendar;

            items.Add(new MaintenanceItemViewModel
            {
                TaskId = t.Id,
                Name = t.Name,
                Category = t.Category,
                Notes = t.Notes,
                IntervalKm = t.IntervalKm,
                IntervalMonths = t.IntervalMonths,
                LastDoneKm = lastDoneKm,
                LastDoneUtc = lastDoneUtc,
                NextDueKm = nextDueKm,
                KmRemaining = kmRemaining,
                KmOverdue = kmOverdue,
                NextDueUtc = nextDueUtc,
                Status = status
            });
        }

       
        return items
            .OrderBy(i => StatusOrder(i.Status))
            .ThenByDescending(i => i.KmOverdue ?? int.MinValue)
            .ThenBy(i => i.KmRemaining ?? int.MaxValue)
            .ToList();
    }

    private static int StatusOrder(MaintenanceStatus s) => s switch
    {
        MaintenanceStatus.Overdue => 0,
        MaintenanceStatus.DueSoon => 1,
        MaintenanceStatus.Ok => 2,
        _ => 3
    };
}
