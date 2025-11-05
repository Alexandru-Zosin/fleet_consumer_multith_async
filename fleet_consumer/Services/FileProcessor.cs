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

        /*
        if (!await ValidateChecksumAsync(_path, fs))
        {

            var errorDir = Path.Combine(PathConfig.FilesDir, "errorFiles");
            Directory.CreateDirectory(errorDir);
            var destPath = Path.Combine(errorDir, Path.GetFileName(_path));

            await fs.DisposeAsync(); // release file handle before moving it and its metadata
            File.Move(_path, destPath, true);
            File.Move(_path + ".meta.json", destPath + ".meta.json", true);
            await File.AppendAllTextAsync(
                Path.Combine(errorDir, "errors.log"),
                $"{DateTime.UtcNow:o} | Checksum invalid for {_path}{Environment.NewLine}");

            return;
        }
        */
        
        var sw = Stopwatch.StartNew();
        int count = 0;
        Console.WriteLine($"File {Path.GetFileName(_path)} started processing (stopwatch started) ... ");

        /*
        await foreach (var dto in JsonDTOReader.ReadArrayFileAsync(_path))
        {
            if (dto == null) continue;
            count++;
            foreach (var kpi in _kpiRegistry.ResolveFor(dto.GetType()))
                kpi.CalculateUntyped(dto);
        }
        */
        IAsyncEnumerator<object?>? enumerator = null;
        try // if file opening throws error, log it and MOVE IT
        {
            enumerator = JsonDTOReader.ReadArrayFileAsync(_path).GetAsyncEnumerator();
        }
        catch (Exception ex)
        {
            var errorDir = Path.Combine(PathConfig.FilesDir, "errorFiles");
            Directory.CreateDirectory(Path.Combine(PathConfig.FilesDir, "errorFiles"));
            await File.AppendAllTextAsync(
                Path.Combine(PathConfig.FilesDir, "errorFiles", "errors.log"),
                $"{DateTime.UtcNow:o} | Enumerator init error: {ex.Message}{Environment.NewLine}");

            var destPath = Path.Combine(errorDir, Path.GetFileName(_path));
            await fs.DisposeAsync(); // release file handle before moving it and its metadata
            File.Move(_path, destPath, true);
            File.Move(_path + ".meta.json", destPath + ".meta.json", true);
            return;
        }

        while (true)
        {
            bool hasNext;
            try // if any specific DTO in that file throws an error 
            {
                hasNext = await enumerator.MoveNextAsync();
            }
            catch (Exception ex)
            {
                await File.AppendAllTextAsync(
                    Path.Combine(PathConfig.FilesDir, "errorFiles", "errors.log"),
                    $"{DateTime.UtcNow:o} | Parse error: {ex.Message}{Environment.NewLine}"
                );
                break; // skip this corrupted file
            }

            if (!hasNext)
                break;

            var dto = enumerator.Current;
            if (dto == null)
                continue;

            count++;
            foreach (var kpi in _kpiRegistry.ResolveFor(dto.GetType()))
                kpi.CalculateUntyped(dto);
        }

        if (enumerator is IAsyncDisposable ad)
            await ad.DisposeAsync(); // disposes the async enumerator

        sw.Stop();
        await File.AppendAllTextAsync(
            Path.Combine(PathConfig.DataDir, "file_metrics.log"),
            $"{Path.GetFileName(_path)} | {sw.ElapsedMilliseconds}ms{Environment.NewLine} | no={count} | {DateTime.UtcNow:o}"
        );
        await fs.DisposeAsync(); // release file descriptor
        File.Delete(_path); // delete the file after successful processing and its metadata
        File.Delete(_path + ".meta.json");
    }

    private static async Task<FileStream?> TryOpenWithRetryAsync(string path)
    {
        int retries = 3;
        var delay = TimeSpan.FromMilliseconds(750);

        for (int i = 0; i < retries; i++)
        {
            try
            {
                return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception ex)
            {
                var errorDir = Path.Combine(PathConfig.FilesDir, "errorFiles");
                Directory.CreateDirectory(errorDir);
                await File.AppendAllTextAsync(
                    Path.Combine(PathConfig.FilesDir, "errorFiles", "errors.log"),
                    $"{DateTime.UtcNow:o} | File open failed try {i + 1}: {ex.Message}{Environment.NewLine}"
                );

                await Task.Delay(delay); // RETRIES with backoffs
            }
        }

        await File.AppendAllTextAsync(
            Path.Combine(PathConfig.FilesDir, "errorFiles", "errors.log"),
            $"File open failed after {retries} attempts{Environment.NewLine}"
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

        return false;
    }
}
