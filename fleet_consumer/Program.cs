using System;
using Services;
using Config.InboxWatch;
using KPI;
// TODO: Save + resume KPI progress
using Config;

var cfg = InboxWatcherConfig.LoadJsonFromFile(
    Path.Combine(PathConfig.BaseDir, "Config", "InboxWatch", "FolderWatchConfig.json"));

var kpis = new IKPI[]
{
    new KpiAverageLoad(),
    new KpiAverageFuelUsedPerVehicle(),
    new KpiPeakLoad(),
    new KpiSuccessfulDeliveries()
};

foreach (var kpi in kpis)
    await kpi.DeserializeFromFileAsync(Path.Combine(PathConfig.DataDir, "KPICheckpointSave"));

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true; // prevent abrupt termination
    cts.Cancel();    // signal graceful cancellation
};

var kpiRegistry = new KPIRegistry(kpis);
var worker = new PipelineWorker(kpiRegistry);
using var watcher = new InboxWatcher(cfg, worker, cts);

Console.WriteLine("Watching folder. Press CtrlC to exit.");

await watcher.StartAsync();
foreach (var kpi in kpis)
{
    await kpi.SerializeAsync(Path.Combine(PathConfig.DataDir, "KPICheckpointSave"));
    await kpi.WriteMetricToFileAsync(Path.Combine(PathConfig.DataDir));
}