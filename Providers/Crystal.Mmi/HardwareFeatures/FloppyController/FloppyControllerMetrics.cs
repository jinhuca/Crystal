namespace Crystal.Mmi.HardwareFeatures.FloppyController;

// Win32_FloppyController is derived from CIM_Controller (like IDEController/SCSIController),
// so it carries the generic controller field set (MaxNumberControlled/ProtocolSupported/
// TimeOfLastReset) rather than any floppy-specific properties of its own.
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
