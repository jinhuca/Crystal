namespace Crystal.Mmi.HardwareFeatures.Volume;

public record VolumeMetrics(
    string? Name,
    string? Caption,
    string? Description,
    string? DeviceID,
    ulong? Capacity,
    ulong? FreeSpace,
    uint? BlockSize,
    string? DriveLetter,
    ushort? DriveType,
    string? Label,
    string? FileSystem,
    string? SerialNumber,
    bool? Automount,
    bool? BootVolume,
    bool? SystemVolume,
    bool? Compressed,
    bool? DirtyBitSet,
    bool? IndexingEnabled,
    bool? PageFilePresent,
    bool? QuotasEnabled,
    bool? QuotasIncomplete,
    bool? QuotasRebuilding,
    bool? SupportsDiskQuotas,
    bool? SupportsFileBasedCompression,
    ushort? Availability,
    uint? ConfigManagerErrorCode,
    bool? ConfigManagerUserConfig,
    string? CreationClassName,
    bool? ErrorCleared,
    string? ErrorDescription,
    string? ErrorMethodology,
    DateTime? InstallDate,
    uint? LastErrorCode,
    string? PNPDeviceID,
    ushort[]? PowerManagementCapabilities,
    bool? PowerManagementSupported,
    string? Purpose,
    string? Status,
    ushort? StatusInfo,
    string? SystemCreationClassName,
    string? SystemName,
    uint? MaximumFileNameLength)
{
    public double? CapacityInGB => Capacity is null ? null : Math.Round(Capacity.Value / 1024d / 1024d / 1024d, 2);
    public double? FreeSpaceInGB => FreeSpace is null ? null : Math.Round(FreeSpace.Value / 1024d / 1024d / 1024d, 2);
    public double? UsedPercent => Capacity > 0 && FreeSpace is not null ? Math.Round((Capacity.Value - FreeSpace.Value) * 100d / Capacity.Value, 2) : null;
    public double? FreePercent => Capacity > 0 && FreeSpace is not null ? Math.Round(FreeSpace.Value * 100d / Capacity.Value, 2) : null;
    public string? DriveTypeName => DriveType switch { 0 => "Unknown", 1 => "No Root Directory", 2 => "Removable Disk", 3 => "Local Disk", 4 => "Network Drive", 5 => "Compact Disc", 6 => "RAM Disk", _ => null };
}
