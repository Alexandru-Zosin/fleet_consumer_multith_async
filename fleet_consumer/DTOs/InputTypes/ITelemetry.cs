namespace DTO.InputType;

public interface ITelemetry
{
	public DateTime Timestamp { get; set; }
	public string? FleetId { get; set; }
	public string? VehicleId { get; set; }
	public DeliveryStatus DeliveryStatus { get; set; }
	public List<int>? DeliveryList { get; set; }
	public string? SchemaVersion { get; set; }
	public List<int>? WhatWasAdded { get; set; }
}