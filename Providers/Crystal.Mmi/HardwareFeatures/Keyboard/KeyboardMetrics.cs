namespace Crystal.Mmi.HardwareFeatures.Keyboard;

public record KeyboardMetrics(
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
  bool? IsLocked,
  uint? LastErrorCode,
  string? Layout,
  string? Name,
  ushort? NumberOfFunctionKeys,
  ushort? Password,
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName
);
