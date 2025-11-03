using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using Config;

namespace KPI;

public static class KPICalculator
{
    public static readonly ConcurrentDictionary<string, (double, int)> State = new();

    public static async Task SaveAsync()
    {
        var options = new JsonSerializerOptions
        {
            Converters = { new TupleConverter() },
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(State, options);
        await File.WriteAllTextAsync(Path.Combine(PathConfig.FilesDir, "kpi_state.json"), json);
    }

    public static async Task LoadAsync()
    {
        var file = Path.Combine(PathConfig.FilesDir, "kpi_state.json");
        if (!File.Exists(file))
            return;

        var json = await File.ReadAllTextAsync(file);
        if (string.IsNullOrWhiteSpace(json))
            return;

        var options = new JsonSerializerOptions
        {
            Converters = { new TupleConverter() }
        };

        var dict = JsonSerializer.Deserialize<Dictionary<string, (double, int)>>(json, options);
        if (dict == null || dict.Count == 0)
            return;

        foreach (var kv in dict)
            State[kv.Key] = kv.Value;
    }

    private sealed class TupleConverter : JsonConverter<(double, int)>
    {
        public override (double, int) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            double item1 = 0;
            int item2 = 0;

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string name = reader.GetString()!;
                    reader.Read();
                    switch (name)
                    {
                        case "Item1":
                            item1 = reader.GetDouble();
                            break;
                        case "Item2":
                            item2 = reader.GetInt32();
                            break;
                    }
                }
            }
            return (item1, item2);
        }

        public override void Write(Utf8JsonWriter writer, (double, int) value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("Item1", value.Item1);
            writer.WriteNumber("Item2", value.Item2);
            writer.WriteEndObject();
        }
    }
}
