using System.Collections.Concurrent;
using DTO.InputType;
using DTO;
namespace KPI;


public sealed class KpiSuccessfulDeliveries : KPIBase<ITelemetry>
{
    public override string Name => "SuccessfulDeliveries";

    public override void Calculate(ITelemetry t, ConcurrentDictionary<string, (double, int)> state)
    {
        if (t.DeliveryStatus != DeliveryStatus.Completed)
            return;

        state.AddOrUpdate(Name,
            _ => (1.0, 1),
            (_, old) => (old.Item1 + 1.0, old.Item2 + 1));
    }
}