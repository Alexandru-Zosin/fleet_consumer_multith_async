using System.Collections.Concurrent;
using DTO.InputType;

namespace KPI;

public sealed class KpiAverageFuelUsedPerVehicle : KPIBase<IGPS>
{
    public override string Name => "AverageFuelUsedPerVehicle";

    public override void Calculate(IGPS t, ConcurrentDictionary<string, (double, int)> state)
    {
        state.AddOrUpdate(Name,
            _ => (t.FuelPct, 1),
            (_, old) => (old.Item1 + t.FuelPct, old.Item2 + 1));
    }
}
