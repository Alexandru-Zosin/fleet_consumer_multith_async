namespace DTO;

[SchemaVersion("v1")]
public sealed class DeliveryRecordV1 : DeliveryRecordBase // (ITelemetry as well)
{
    public override IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (!string.Equals(SchemaVersion, "v1", StringComparison.Ordinal))
            errors.Add("schemaVersion must be 'v1' for DeliveryRecordV1.");

        ValidateCommon(errors);
        return errors;
    }
}

