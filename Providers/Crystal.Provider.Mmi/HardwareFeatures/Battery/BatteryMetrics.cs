namespace Crystal.Provider.Mmi.HardwareFeatures.Battery;

/// <summary>
/// Represents the metrics of a battery, including its status, capacity, and other relevant information.
/// </summary>
/// <param name="Availability">The availability of the battery.</param>
/// <param name="BatteryRechargeTime">The time required to recharge the battery.</param>
/// <param name="BatteryStatus">The status of the battery.</param>
/// <param name="Caption">The caption of the battery.</param>
/// <param name="Chemistry">The chemistry of the battery.</param>
/// <param name="ConfigManagerErrorCode">The error code from the configuration manager.</param>
/// <param name="ConfigManagerUserConfig">Indicates whether the configuration is user-defined.</param>
/// <param name="CreationClassName">The class name of the creation.</param>
/// <param name="Description">The description of the battery.</param>
/// <param name="DesignCapacity">The design capacity of the battery.</param>
/// <param name="DesignVoltage">The design voltage of the battery.</param>
/// <param name="DeviceID">The device ID of the battery.</param>
/// <param name="ErrorCleared">Indicates whether the error is cleared.</param>
/// <param name="ErrorDescription">The description of the error.</param>
/// <param name="EstimatedChargeRemaining">The estimated charge remaining of the battery.</param>
/// <param name="EstimatedRunTime">The estimated run time of the battery.</param>
/// <param name="ExpectedBatteryLife">The expected battery life.</param>
/// <param name="ExpectedLife">The expected life.</param>
/// <param name="FullChargeCapacity">The full charge capacity of the battery.</param>
/// <param name="InstallDate">The installation date of the battery.</param>
/// <param name="LastErrorCode">The last error code.</param>
/// <param name="MaxRechargeTime">The maximum recharge time of the battery.</param>
/// <param name="Name">The name of the battery.</param>
/// <param name="PNPDeviceID">The PNP device ID of the battery.</param>
/// <param name="PowerManagementCapabilities">The power management capabilities of the battery.</param>
/// <param name="PowerManagementSupported">Indicates whether power management is supported.</param>
/// <param name="SmartBatteryVersion">The version of the smart battery.</param>
/// <param name="Status">The status of the battery.</param>
/// <param name="StatusInfo">The status information of the battery.</param>
/// <param name="SystemCreationClassName">The class name of the system creation.</param>
/// <param name="SystemName">The name of the system.</param>
/// <param name="TimeOnBattery">The time the battery has been on.</param>
/// <param name="TimeToFullCharge">The time required to fully charge the battery.</param>
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
