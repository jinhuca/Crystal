namespace Crystal.Provider.Mmi.HardwareFeatures.Battery;

public record BatteryMetrics(
  ushort? Availability,
  uint? BatteryRechargeTime,
  ushort? BatteryStatus,
  string? Caption,
  ushort? Chemistry,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  string? Description,
  uint? DesignCapacity,          // mWh
  ulong? DesignVoltage,           // mV
  string? DeviceID,
  bool? ErrorCleared,
  string? ErrorDescription,
  ushort? EstimatedChargeRemaining, // percentage (0-100)
  uint? EstimatedRunTime,         // minutes
  uint? ExpectedBatteryLife,      // minutes
  uint? ExpectedLife,             // minutes
  uint? FullChargeCapacity,       // mWh
  DateTime? InstallDate,
  uint? LastErrorCode,
  uint? MaxRechargeTime,          // minutes
  string? Name,
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  string? SmartBatteryVersion,
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName,
  uint? TimeOnBattery,            // seconds
  uint? TimeToFullCharge          // minutes
) {
  /// <summary>
  /// Human-readable description of <see cref="BatteryStatus"/>.
  /// </summary>
  public string? BatteryStatusPhrase => BatteryStatus switch {
    1 => "Other",
    2 => "Unknown",
    3 => "Fully Charged",
    4 => "Low",
    5 => "Critical",
    6 => "Charging",
    7 => "Charging and High",
    8 => "Charging and Low",
    9 => "Charging and Critical",
    10 => "Undefined",
    11 => "Partially Charged",
    _ => null
  };

  /// <summary>
  /// Human-readable description of <see cref="Chemistry"/>.
  /// </summary>
  public string? ChemistryName => Chemistry switch {
    1 => "Other",
    2 => "Unknown",
    3 => "Lead Acid",
    4 => "Nickel Cadmium",
    5 => "Nickel Metal Hydride",
    6 => "Lithium-ion",
    7 => "Zinc Air",
    8 => "Lithium Polymer",
    _ => null
  };
}
