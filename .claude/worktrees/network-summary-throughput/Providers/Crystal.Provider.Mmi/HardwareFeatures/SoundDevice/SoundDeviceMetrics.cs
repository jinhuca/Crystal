namespace Crystal.Provider.Mmi.HardwareFeatures.SoundDevice;
public record SoundDeviceMetrics(
  ushort? Availability,
  string? Caption,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  string? Description,
  string? DeviceID,            // Key unique identifier string
  ushort? DMABufferSize,
  bool? ErrorCleared,
  string? ErrorDescription,
  DateTime? InstallDate,
  uint? LastErrorCode,
  string? Manufacturer,        // e.g., "Realtek", "NVIDIA", "Focusrite"
  string? MPU401Address,
  string? Name,                // Audio controller name (e.g., "Realtek High Definition Audio")
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  string? ProductID,
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName
);
