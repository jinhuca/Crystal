namespace Crystal.Provider.Mmi.HardwareFeatures.SystemSlot;

public record SystemSlotMetrics(
    string? Caption, string? ConnectorPinout, ushort[]? ConnectorType, string? CreationClassName, ushort? CurrentUsage,
    string? Description, float? HeightAllowed, DateTime? InstallDate, float? LengthAllowed, string? Manufacturer,
    ushort? MaxDataWidth, string? Model, string? Name, ushort? Number, string? OtherIdentifyingInfo,
    string? PartNumber, bool? PMESignal, bool? PoweredOn, string? PurposeDescription, string? SerialNumber,
    bool? Shared, string? SKU, string? SlotDesignation, bool? SpecialPurpose, string? Status,
    bool? SupportsHotPlug, string? Tag, uint? ThermalRating, ushort[]? VccMixedVoltageSupport,
    string? Version, ushort[]? VppMixedVoltageSupport)
{
    public string? CurrentUsageName => CurrentUsage switch { 1 => "Other", 2 => "Unknown", 3 => "Available", 4 => "In Use", 5 => "Unavailable", _ => null };
    public string? SlotWidthName => MaxDataWidth switch { 1 => "Other", 2 => "Unknown", 3 => "8-bit", 4 => "16-bit", 5 => "32-bit", 6 => "64-bit", 7 => "128-bit", _ => null };
}
