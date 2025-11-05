using Config.InboxWatch;
using Services.Interfaces;

namespace Services;

public sealed class InboxWatcher : IDisposable
{
    private readonly IPipelineWorker _worker;
    private readonly InboxWatcherConfig _cfg;
    private readonly FileSystemWatcher _folderWatch;
    private readonly Timer _rescanFolderTaskTimer;
    private readonly CancellationTokenSource _cts;

    public InboxWatcher(InboxWatcherConfig cfg, IPipelineWorker worker, CancellationTokenSource cts)
    {
        _cfg = cfg;
        _worker = worker;
        _cts = cts;
        _folderWatch = new FileSystemWatcher(_cfg.Path!)
        {
           // Filter = _cfg.FileType!,
            IncludeSubdirectories = _cfg.IncludeSubdirectories ?? false,
            EnableRaisingEvents = true
        };

        _folderWatch.Renamed += async (_, eventArgs) =>
        {
       //     if (Path.GetExtension(eventArgs.OldFullPath).Equals(".tmp", StringComparison.OrdinalIgnoreCase) &&
       //         Path.GetExtension(eventArgs.FullPath).Equals(".jsonl", StringComparison.OrdinalIgnoreCase))
        //    {
                await _worker.EnqueueIfNewAsync(NormalizePath(eventArgs.FullPath), _cts.Token);
       //     }
        };
    }

    private void RescanFolder()
    {
        var searchOption = _cfg.IncludeSubdirectories == true
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;
        var pattern = $"*.{_cfg.FileType!.TrimStart('.')}";
        // DEBUG here

        foreach (var f in Directory.EnumerateFiles(_cfg.Path!, pattern, searchOption))
            _ = _worker.EnqueueIfNewAsync(f, _cts.Token);
    }

    public async Task StartAsync()
    {
        await _worker.ChannelLoop(_cts.Token);
    }

    public void Stop()
    {
        _cts.Cancel();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    private static string NormalizePath(string path) =>
        Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
}