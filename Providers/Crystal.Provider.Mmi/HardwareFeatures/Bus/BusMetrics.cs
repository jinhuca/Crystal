namespace Crystal.Provider.Mmi.HardwareFeatures.Bus;

/// <summary>
/// Represents the metrics of a bus in the system, including availability, bus number, type, and various configuration and status details.
/// </summary>
/// <param name="Availability">The availability of the bus.</param>
/// <param name="BusNum">The number of the bus.</param>
/// <param name="BusType">The type of the bus.</param>
/// <param name="Caption">The caption of the bus.</param>
/// <param name="ConfigManagerErrorCode">The error code for the configuration manager.</param>
/// <param name="ConfigManagerUserConfig">Indicates if the configuration is user-configured.</param>
/// <param name="CreationClassName">The class name of the creation.</param>
/// <param name="Description">The description of the bus.</param>
/// <param name="DeviceID">The device ID of the bus.</param>
/// <param name="ErrorCleared">Indicates if the error is cleared.</param>
/// <param name="ErrorDescription">The description of the error.</param>
/// <param name="InstallDate">The installation date of the bus.</param>
/// <param name="LastErrorCode">The last error code.</param>
/// <param name="Name">The name of the bus.</param>
/// <param name="PNPDeviceID">The PNP device ID of the bus.</param>
/// <param name="PowerManagementCapabilities">The power management capabilities of the bus.</param>
/// <param name="PowerManagementSupported">Indicates if power management is supported.</param>
/// <param name="Status">The status of the bus.</param>
/// <param name="StatusInfo">The status information of the bus.</param>
/// <param name="SystemCreationClassName">The class name of the system creation.</param>
/// <param name="SystemName">The name of the system.</param>
public record BusMetrics(
  ushort? Availability,
  uint? BusNum,
  uint? BusType,
  string? Caption,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  string? Description,
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
  string? SystemName
);
