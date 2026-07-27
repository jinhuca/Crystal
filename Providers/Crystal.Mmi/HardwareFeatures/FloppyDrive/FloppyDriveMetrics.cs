namespace Crystal.Mmi.HardwareFeatures.FloppyDrive;

// Win32_FloppyDrive is derived from CIM_DisketteDrive -> CIM_MediaAccessDevice, so it
// carries the generic removable-media-device field set (Capabilities/MaxMediaSize/
// NeedsCleaning/etc.) rather than any floppy-specific properties of its own.
public record FloppyDriveMetrics(
  ushort? Availability,
  ushort[]? Capabilities,
  string[]? CapabilityDescriptions,
  string? Caption,
  ushort? CompressionMethod,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  ulong? DefaultBlockSize,
  string? Description,
  string? DeviceID,
  bool? ErrorCleared,
  string? ErrorDescription,
  string? ErrorMethodology,
  DateTime? InstallDate,
  uint? LastErrorCode,
  string? Manufacturer,
  ulong? MaxBlockSize,
  ulong? MaxMediaSize,
  ulong? MinBlockSize,
  string? Name,
  bool? NeedsCleaning,
  uint? NumberOfMediaSupported,
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName
);
