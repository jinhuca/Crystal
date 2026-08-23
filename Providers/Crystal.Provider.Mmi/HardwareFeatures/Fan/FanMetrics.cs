namespace Crystal.Provider.Mmi.HardwareFeatures.Fan;

/// <summary>
/// Represents the metrics of a fan device, including its operational status, speed, and other relevant properties.
/// </summary>
/// <param name="ActiveCooling">Indicates whether the fan is actively cooling.</param>
/// <param name="Availability">The availability of the fan.</param>
/// <param name="Caption">A short description of the fan.</param>
/// <param name="ConfigManagerErrorCode">The error code for the configuration manager.</param>
/// <param name="ConfigManagerUserConfig">Indicates whether the user has configured the fan.</param>
/// <param name="CreationClassName">The name of the class that created the instance.</param>
/// <param name="Description">A detailed description of the fan.</param>
/// <param name="DesiredSpeed">The desired speed of the fan.</param>
/// <param name="DeviceID">The device ID of the fan.</param>
/// <param name="ErrorCleared">Indicates whether the error has been cleared.</param>
/// <param name="ErrorDescription">A description of the error.</param>
/// <param name="InstallDate">The date when the fan was installed.</param>
/// <param name="LastErrorCode">The error code for the last error.</param>
/// <param name="Name">The name of the fan.</param>
/// <param name="PNPDeviceID">The PNP device ID of the fan.</param>
/// <param name="PowerManagementCapabilities">The capabilities of the power management system.</param>
/// <param name="PowerManagementSupported">Indicates whether power management is supported.</param>
/// <param name="Status">The status of the fan.</param>
/// <param name="StatusInfo">Additional information about the fan's status.</param>
/// <param name="SystemCreationClassName">The name of the class that created the system instance.</param>
/// <param name="SystemName">The name of the system.</param>
/// <param name="VariableSpeed">Indicates whether the fan has variable speed control.</param>
public record FanMetrics(
  bool? ActiveCooling,
  ushort? Availability,
  string? Caption,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  string? Description,
  ulong? DesiredSpeed,          // RPM, meaningful only when VariableSpeed is true
  string? DeviceID,
  bool? ErrorCleared,
  string? ErrorDescription,
  DateTime? InstallDate,
  uint? LastErrorCode,
  string? Name,
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName,
  bool? VariableSpeed
);
