using DTO;
using System.Text.Json;

namespace Services;

public static class JsonDTOReader
{
    // Optionally cache options to avoid per-call allocation.
    private static readonly JsonSerializerOptions _jsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    public static async IAsyncEnumerable<object?> ReadArrayFileAsync(string path)
    {
        var json = await File.ReadAllTextAsync(path);
        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.ValueKind != JsonValueKind.Array)
            yield break;

        foreach (var element in doc.RootElement.EnumerateArray())
        {
            object? obj = null;
            try
            {
                if (!element.TryGetProperty("schemaVersion", out var schemaProp))
                    continue;

                var schema = schemaProp.GetString();
                if (schema is null)
                    continue;

                if (!DTORegistry.Schemas.TryGetValue(schema, out var dtoType))
                    continue;

                obj = JsonSerializer.Deserialize(element.GetRawText(), dtoType, _jsonOptions);
            }
            catch (Exception ex)
            {
                await File.AppendAllTextAsync(
                    "errors.log",
                    $"{DateTime.UtcNow:o} | Parse error: {ex.Message}{Environment.NewLine}"
                );
            }

            if (obj is not null)
                yield return obj;
        }
    }
}
