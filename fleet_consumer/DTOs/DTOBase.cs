using System.Text.Json.Serialization;
using DTO.InputType;

namespace DTO;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DeliveryStatus
{
    PickUp,
    InProgress,
    Completed
}

public abstract class DeliveryRecordBase : ITelemetry
{
    [JsonPropertyName("tsUtc")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("fleetId")]
    public string? FleetId { get; set; }

    [JsonPropertyName("vehicleId")]
    public string? VehicleId { get; set; }

    [JsonPropertyName("deliveryStatus")]
    public DeliveryStatus DeliveryStatus { get; set; }

    [JsonPropertyName("deliveryList")]
    public List<int>? DeliveryList { get; set; }

    [JsonPropertyName("schemaVersion")]
    public string? SchemaVersion { get; set; }

    [JsonPropertyName("whatWasAdded")]
    public List<int>? WhatWasAdded { get; set; }

    // shared checks
    protected void ValidateCommon(List<string> errors)
    {
        if (DeliveryList is null)
            errors.Add("deliveryList is required.");

        else if (!HasUniqueItems(DeliveryList))
            errors.Add("deliveryList must contain unique items.");

        if (WhatWasAdded is not null && !HasUniqueItems(WhatWasAdded))
            errors.Add("whatWasAdded must contain unique items when present.");
    }

    protected static bool HasUniqueItems(List<int> list)
    {
        var set = new HashSet<int>();
        foreach (var v in list)
        {
            if (!set.Add(v)) return false;
        }
        return true;
    }

    // DTO version-specific rules
    public abstract IReadOnlyList<string> Validate();
}
