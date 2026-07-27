namespace Crystal.Mmi.HardwareFeatures.PCMCIAController;

// Win32_PCMCIAController is derived from CIM_PCMCIAController -> CIM_Controller, and (like
// IDEController/SCSIController/FloppyController) adds no device-specific properties beyond
// the generic controller field set.
public record PCMCIAControllerMetrics(
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
