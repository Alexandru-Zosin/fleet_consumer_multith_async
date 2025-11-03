using System.Collections.Concurrent;
using DTO.InputType;

namespace KPI;

public sealed class KpiPeakLoad : KPIBase<ITelemetry>
{
    public override string Name => "PeakLoad";

    public override void Calculate(ITelemetry t, ConcurrentDictionary<string, (double, int)> state)
    {
        int load = t.DeliveryList?.Count ?? 0;

        state.AddOrUpdate(Name,
            _ => (load, 1),
            (_, old) => (Math.Max(old.Item1, load), old.Item2 + 1));
    }
}