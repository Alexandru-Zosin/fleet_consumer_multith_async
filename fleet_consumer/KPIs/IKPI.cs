using System;
using System.Collections.Concurrent;

namespace KPI;

public interface IKPI
{
    Type InputType { get; }
    string Name { get; }
    void CalculateUntyped(object telemetry, ConcurrentDictionary<string, (double, int)> state);
}

public interface IKPI<in T> : IKPI // contravariance
{
    void Calculate(T telemetry, ConcurrentDictionary<string, (double, int)> state);
}