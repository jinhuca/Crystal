namespace Crystal.Provider.Mmi.HardwareFeatures.PhysicalMemory;
public record PhysicalMemoryMetrics(
    ushort? Attributes,
    string? BankLabel,            // e.g., "BANK 0", "BANK 1"
    ulong? Capacity,              // Total capacity of this stick in bytes
    string? Caption,
    uint? ConfiguredClockSpeed,   // Actual operating speed of the RAM in MHz
    uint? ConfiguredVoltage,      // Operating voltage in millivolts
    string? CreationClassName,
    ushort? DataWidth,
    string? Description,
    string? DeviceLocator,        // The slot location on the board (e.g., "DIMM A", "Slot 1")
    ushort? FormFactor,           // 8 = DIMM, 12 = SODIMM (Laptop RAM)
    bool? HotSwappable,
    DateTime? InstallDate,
    ushort? InterleaveDataDepth,
    uint? InterleavePosition,
    string? Manufacturer,         // e.g., "Crucial", "Corsair", "Kingston"
    uint? MaxVoltage,
    ushort? MemoryType,           // Older type identifier enum
    uint? MinVoltage,
    string? Model,
    string? Name,
    string? OtherIdentifyingInfo,
    string? PartNumber,           // Manufacturer product number string
    uint? PositionInRow,
    bool? PoweredOn,
    bool? Removable,
    bool? Replaceable,
    string? SerialNumber,         // RAM stick unique tracking serial string
    string? SKU,
    ushort? Speed,                // Maximum advertised speed of the module in MHz
    string? Status,
    string? Tag,
    ushort? TotalWidth,
    ushort? TypeDetail,           // 128 = Synchronous, 4 = Non-volatile, etc.
    string? Version
) {
  // --- RUNTIME PRESENTATION HELPERS ---

  // Translates the standard WMI FormFactor enum into a human-readable name
  public string FormFactorName => FormFactor switch {
    8 => "DIMM (Desktop)",
    12 => "SODIMM (Laptop)",
    13 => "Row of chips",
    15 => "SIMM",
    _ => "Unknown Form Factor"
  };

  // Computes individual capacity into a clean gigabytes representation format
  public double? CapacityInGB => Capacity.HasValue
      ? Math.Round(Capacity.Value / 1024.0 / 1024.0 / 1024.0, 1)
      : null;
}
