using System.Threading.Channels;
using System.Collections.Concurrent;
using Services.Interfaces;
using KPI;
using Config;
using System.Diagnostics;

namespace Services;

public sealed class PipelineWorker : IPipelineWorker
{
    private readonly KPIRegistry _kpiRegistry;
    private readonly SemaphoreSlim _concurrencySemaphore;
    private readonly Channel<string> _channel;
    private readonly ConcurrentDictionary<string, byte> _pendingInChannel;

    public PipelineWorker(
        KPIRegistry kpiRegistry,
        int boundedCapacity = 100)
    {
        _kpiRegistry = kpiRegistry;
        _concurrencySemaphore = new SemaphoreSlim(Environment.ProcessorCount);
        _channel = Channel.CreateBounded<string>(boundedCapacity);
        _pendingInChannel = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
    }

    public async Task EnqueueIfNewAsync(string path, CancellationToken cts)
    {
        if (!File.Exists(path)) return;
        if (!_pendingInChannel.TryAdd(path, 0)) return;

        try
        {
            await _channel.Writer.WriteAsync(path, cts).ConfigureAwait(false);
        }
        catch (ChannelClosedException)
        {
            _pendingInChannel.TryRemove(path, out _);
            // add the file again to be reprocessed if the _channel gets closed
        }
    }

    public async Task ChannelLoop(CancellationToken cts)
    {
        await KPICalculator.LoadAsync();
        var sw = Stopwatch.StartNew();
        try
        {
            await foreach (var file in _channel.Reader.ReadAllAsync(cts))
            {
                await _concurrencySemaphore.WaitAsync(cts);
                // bounding processFile to avoid I/O flood

                /*
                * I/O-bound operations: Use async/await without Task.Run
                    CPU-bound operations: Use Task.Run to move work to background thread
                    Mixed CPU/I/O: Use Task.Run(async () => {}) to move the entire operation to background
                to read:
                https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios#recognize-cpu-bound-and-io-bound-scenarios%22https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios#recognize-cpu-bound-and-io-bound-scenarios%22
                */
                await new FileProcessor(file, _kpiRegistry).ProcessAsync();

                /*
                 _ = Task.Run(async () =>
            {
                try
                {
                    cts.ThrowIfCancellationRequested();
                    await new FileProcessor(file, _kpiRegistry).ProcessAsync();
                }
                finally
                {
                    _pendingInChannel.TryRemove(file, out _);
                    _concurrencySemaphore.Release();
                }
            });
                */
                _concurrencySemaphore.Release();
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            sw.Stop();

            await KPICalculator.SaveAsync();

            var logLine = $"Entire operation duration={sw.ElapsedMilliseconds}ms{Environment.NewLine}";
            var logPath = Path.Combine(PathConfig.DataDir, "file_metrics.log");
            await File.AppendAllTextAsync(logPath, logLine);
        }
    }

    public void Dispose()
    {
        _channel.Writer.TryComplete();
        _concurrencySemaphore.Dispose();
    }
}
