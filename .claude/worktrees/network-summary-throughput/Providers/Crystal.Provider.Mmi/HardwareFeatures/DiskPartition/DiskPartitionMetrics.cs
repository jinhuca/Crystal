namespace Crystal.Provider.Mmi.HardwareFeatures.DiskPartition;

public record DiskPartitionMetrics(
  ushort? Availability,
  bool? Bootable,
  bool? BootPartition,
  string? Caption,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  ulong? BlockSize,            // Maps from CIM_StorageExtent.BlockSize (uint64)
  string? Description,
  string? DeviceID,            // Key identifier (e.g., "Disk #0, Partition #1")
  uint? DiskIndex,
  bool? ErrorCleared,
  string? ErrorDescription,
  string? ErrorMethodology,
  uint? Index,
  DateTime? InstallDate,
  uint? LastErrorCode,
  string? Name,
  ulong? NumberOfBlocks,       // Maps from CIM_StorageExtent.NumberOfBlocks (uint64)
  bool? PrimaryPartition,
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  string? Purpose,
  ulong? Size,                 // Partition capacity in bytes (uint64)
  ulong? StartingOffset,       // Partition start address (uint64)
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName,
  ushort? TargetOperatingSystem,
  ushort? Type                 // e.g., 12 = GPT, 1 = Extended, etc.
);
