namespace Crystal.Provider.Mmi.HardwareFeatures.FloppyController;

/// <summary>
/// Represents the metrics of a floppy controller, derived from the WMI class <c>Win32_FloppyController</c>.
///  Win32_FloppyController is derived from CIM_Controller (like IDEController/SCSIController),
/// so it carries the generic controller field set (MaxNumberControlled/ProtocolSupported/
/// TimeOfLastReset) rather than any floppy-specific properties of its own.
/// </summary>
/// <param name="Availability">The availability of the floppy controller.</param>
/// <param name="Caption">The caption for the floppy controller.</param>
/// <param name="ConfigManagerErrorCode">The configuration manager error code.</param>
/// <param name="ConfigManagerUserConfig">Indicates if the configuration is user-configured.</param>
/// <param name="CreationClassName">The creation class name.</param>
/// <param name="Description">The description of the floppy controller.</param>
/// <param name="DeviceID">The device ID.</param>
/// <param name="ErrorCleared">Indicates if the error is cleared.</param>
/// <param name="ErrorDescription">The error description.</param>
/// <param name="InstallDate">The installation date.</param>
/// <param name="LastErrorCode">The last error code.</param>
/// <param name="Manufacturer">The manufacturer of the floppy controller.</param>
/// <param name="MaxNumberControlled">The maximum number of devices that can be controlled.</param>
/// <param name="Name">The name of the floppy controller.</param>
/// <param name="PNPDeviceID">The PNP device ID.</param>
/// <param name="PowerManagementCapabilities">The power management capabilities.</param>
/// <param name="PowerManagementSupported">Indicates if power management is supported.</param>
/// <param name="ProtocolSupported">The protocol supported.</param>
/// <param name="Status">The status of the floppy controller.</param>
/// <param name="StatusInfo">The status information.</param>
/// <param name="SystemCreationClassName">The system creation class name.</param>
/// <param name="SystemName">The system name.</param>
/// <param name="TimeOfLastReset">The time of the last reset.</param>
public record FloppyControllerMetrics(
  ushort? Availability,
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
  string? Manufacturer,
  uint? MaxNumberControlled,
  string? Name,
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  ushort? ProtocolSupported,
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName,
  DateTime? TimeOfLastReset
);
