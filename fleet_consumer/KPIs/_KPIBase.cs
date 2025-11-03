using System.Collections.Concurrent;

namespace KPI;

public abstract class KPIBase<T> : IKPI<T>
{
    public abstract string Name { get; }
    public Type InputType => typeof(T);
    public abstract void Calculate(T telemetry, ConcurrentDictionary<string, (double, int)> state);
    public void CalculateUntyped(object telemetry, ConcurrentDictionary<string, (double, int)> state)
        => Calculate((T)telemetry, state);
}