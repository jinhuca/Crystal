using System;

namespace Crystal.Mmi.HardwareFeatures.MappedLogicalDisk;

// Win32_MappedLogicalDisk is derived from CIM_LogicalDisk, like Win32_LogicalDisk, but
// represents network storage mapped as a logical disk for the querying user's logon
// session specifically (ProviderName / SessionID identify the UNC share and session).
public record MappedLogicalDiskMetrics(
  ushort? Access,
  ushort? Availability,
  ulong? BlockSize,
  string? Caption,
  bool? Compressed,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  string? Description,
  string? DeviceID,            // Key identifier (e.g., "Z:")
  bool? ErrorCleared,
  string? ErrorDescription,
  string? ErrorMethodology,
  string? FileSystem,
  ulong? FreeSpace,
  DateTime? InstallDate,
  uint? LastErrorCode,
  uint? MaximumComponentLength,
  string? Name,
  ulong? NumberOfBlocks,
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  string? ProviderName,        // UNC path of the mapped share, e.g. "\\server\share"
  string? Purpose,
  bool? QuotasDisabled,
  bool? QuotasIncomplete,
  bool? QuotasRebuilding,
  string? SessionID,           // Logon session this mapping belongs to
  ulong? Size,
  string? Status,
  ushort? StatusInfo,
  bool? SupportsDiskQuotas,
  bool? SupportsFileBasedCompression,
  string? SystemCreationClassName,
  string? SystemName,
  string? VolumeName,
  string? VolumeSerialNumber
) {
  // --- RUNTIME EXTRACTION CALCULATORS ---

  public double? FreeSpacePercentage => (Size > 0 && FreeSpace.HasValue)
    ? Math.Round(((double)FreeSpace.Value / Size.Value) * 100.0, 2)
    : null;

  public ulong? UsedSpace => (Size.HasValue && FreeSpace.HasValue)
    ? Size.Value - FreeSpace.Value
    : null;
}
