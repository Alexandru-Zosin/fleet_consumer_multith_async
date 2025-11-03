using System.Text.Json.Serialization;
using DTO.InputType;
namespace DTO;

[SchemaVersion("v2")]
public sealed class DeliveryRecordV2 : DeliveryRecordBase, IGPS
{
    [JsonPropertyName("odometer")]
    public double Odometer { get; set; }

    [JsonPropertyName("fuelPct")]
    public double FuelPct { get; set; }

    public override IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (!string.Equals(SchemaVersion, "v2", StringComparison.Ordinal))
            errors.Add("schemaVersion must be 'v2' for DeliveryRecordV2.");

        if (!(Odometer > 0))
            errors.Add("odometer must be > 0.");

        if (!(FuelPct > 0))
            errors.Add("Fuel must be > 0.");

        ValidateCommon(errors);
        return errors;
    }
}
