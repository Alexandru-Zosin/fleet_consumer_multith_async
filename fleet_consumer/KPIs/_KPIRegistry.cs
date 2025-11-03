using System.Collections.Concurrent;

namespace KPI;

public sealed class KPIRegistry
{
    private readonly IKPI[] _allKPIs;
    private readonly ConcurrentDictionary<Type, IKPI[]> _cache = new();

    public KPIRegistry(IEnumerable<IKPI> kpis)
    {
        _allKPIs = kpis.ToArray();
    }

    public IReadOnlyList<IKPI> ResolveFor(Type telemetryType)
    {
        return _cache.GetOrAdd(telemetryType, telemType =>
            _allKPIs.Where(kpi => kpi.InputType.IsAssignableFrom(telemType))
            .ToArray()
        );
    }
}