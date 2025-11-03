using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using KPI;
using Config;
namespace Services;

public sealed class FileProcessor
{
    private readonly string _path;
    private readonly KPIRegistry _kpiRegistry;

    public FileProcessor(string path, KPIRegistry kpiRegistry)
    {
        _path = path;
        _kpiRegistry = kpiRegistry;
    }

    public async Task ProcessAsync()
    {
        await using var fs = await TryOpenWithRetryAsync(_path);
        if (fs is null) return;

        //if (!await ValidateChecksumAsync(_path, fs)) return;

        var sw = Stopwatch.StartNew();
        int count = 0;

        await foreach (var dto in JsonDTOReader.ReadArrayFileAsync(_path))
        {
            if (dto == null) continue;
            count++;
            foreach (var kpi in _kpiRegistry.ResolveFor(dto.GetType()))
                kpi.CalculateUntyped(dto, KPICalculator.State);
        }

        sw.Stop();
        await File.AppendAllTextAsync(
            Path.Combine(PathConfig.FilesDir, "file_metrics.log"), 
            $"{DateTime.UtcNow:o} | {_path} | entries={count} | duration={sw.ElapsedMilliseconds}ms{Environment.NewLine}"
        );
    }

    private static async Task<FileStream?> TryOpenWithRetryAsync(string path)
    {
        int retries = 3;
        var delay = TimeSpan.FromMilliseconds(100);

        for (int i = 0; i < retries; i++)
        {
            try
            {
                return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception ex)
            {
                await File.AppendAllTextAsync(
                    "errors.log",
                    $"{DateTime.UtcNow:o} | File open failed try {i + 1}: {ex.Message}{Environment.NewLine}"
                );

                await Task.Delay(delay); // RETRIES with backoffs
            }
        }

        await File.AppendAllTextAsync(
            "errors.log",
            $"{DateTime.UtcNow:o} | File open failed after {retries} attempts{Environment.NewLine}"
        );
        return null;
    }

    private static async Task<bool> ValidateChecksumAsync(string path, Stream stream)
    {
        var sidefile = path + ".meta.json";
        if (!File.Exists(sidefile)) return true;

        using var sha = SHA256.Create();
        var hashBytes = await sha.ComputeHashAsync(stream).ConfigureAwait(false);
        stream.Position = 0;
        var actual = Convert.ToHexString(hashBytes);

        await using var meta = File.OpenRead(sidefile);
        using var doc = await JsonDocument.ParseAsync(meta).ConfigureAwait(false);

        if (!doc.RootElement.TryGetProperty("sha256", out var shaProp))
            return true; // sidefile present but no sha256 field

        var expected = shaProp.GetString();
        if (string.IsNullOrWhiteSpace(expected))
            return true;

        if (actual.Equals(expected.Trim(), StringComparison.OrdinalIgnoreCase))
            return true;

        await File.AppendAllTextAsync(
            "errors.log",
            $"{DateTime.UtcNow:o} | Checksum mismatch for {path}{Environment.NewLine}"
        ).ConfigureAwait(false);

        return false;
    }
}
