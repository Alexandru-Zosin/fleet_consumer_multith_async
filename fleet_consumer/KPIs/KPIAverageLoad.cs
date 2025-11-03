using System.Collections.Concurrent;
using DTO.InputType;

namespace KPI;

public sealed class KpiAverageLoad : KPIBase<ITelemetry>
{
    public override string Name => "AverageLoad";

    public override void Calculate(ITelemetry t, ConcurrentDictionary<string, (double, int)> state)
    {
        var load = t.DeliveryList?.Count ?? 0;

        state.AddOrUpdate(Name,
            _ => (load, 1),
            (_, old) => (old.Item1 + load, old.Item2 + 1));
    }
}
