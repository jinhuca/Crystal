namespace Crystal.Mmi.HardwareFeatures.LogicalDisk;

#nullable enable
using System;

public record LogicalDiskMetrics(
  ushort? Availability,
  ulong? BlockSize,
  string? Caption,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  string? Description,
  string? DeviceID,            // Key identifier (e.g., "C:", "D:")
  uint? DriveType,             // 3 = Fixed Local Disk, 2 = Removable, 5 = Compact Disc
  bool? ErrorCleared,
  string? ErrorDescription,
  string? ErrorMethodology,
  string? FileSystem,          // e.g., "NTFS", "FAT32", "exFAT"
  ulong? FreeSpace,            // Available capacity in bytes
  DateTime? InstallDate,
  uint? LastErrorCode,
  uint? MaximumComponentLength,
  string? Name,
  ulong? NumberOfBlocks,
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  string? ProviderName,        // Mapped remote network share path if applicable
  SafetyStatus? Purpose,
  ulong? Size,                 // Total storage capacity in bytes
  string? Status,
  ushort? StatusInfo,
  bool? SupportsDiskQuotas,
  bool? SupportsFileBasedCompression,
  string? SystemCreationClassName,
  string? SystemName,
  string? VolumeName,          // The user-assigned label of the partition
  string? VolumeSerialNumber
) {
  // --- RUNTIME EXTRACTION CALCULATORS ---

  // Safely computes the percentage of unallocated storage space remaining
  public double? FreeSpacePercentage => (Size > 0 && FreeSpace.HasValue)
    ? Math.Round(((double)FreeSpace.Value / Size.Value) * 100.0, 2)
    : null;

  // Safely computes total utilized disk storage space in bytes
  public ulong? UsedSpace => (Size.HasValue && FreeSpace.HasValue)
    ? Size.Value - FreeSpace.Value
    : null;
}

public enum SafetyStatus : byte { None, Active }
