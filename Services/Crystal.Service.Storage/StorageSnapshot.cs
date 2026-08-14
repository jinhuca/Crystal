namespace Crystal.Service.Storage;

/// <summary>One physical disk drive. <see cref="DriveIndex"/> is the Windows physical-disk number
/// (<c>Win32_DiskDrive.Index</c>, "Disk 0"/"Disk 1"/…) — the key that joins this static inventory
/// to the live per-disk telemetry readings.</summary>
public record StorageDriveInfo(
    string Model,
    double? CapacityGB,
    string? InterfaceType,
    string? MediaType,
    string? Manufacturer,
    string? SerialNumber,
    string? FirmwareRevision,
    uint? Partitions,
    int? DriveIndex);

/// <summary>The system's physical storage: the drives plus rolled-up totals.</summary>
public record StorageSnapshot(
    IReadOnlyList<StorageDriveInfo> Drives,
    double? TotalCapacityGB,
    int DriveCount);

/// <summary>A single physical disk's live activity, matching Task Manager's per-disk Disk page:
/// total-activity percentage (0-100), read/write transfer rates in MB/s, and best-effort average
/// response time in milliseconds (null when the perf counter is unavailable). Temperature (°C) and
/// SSD health/"Life" (percent remaining) come from the drive's SMART sensors and are null when the
/// device doesn't report them or elevation/PawnIO isn't available. Used-space percent and free/total
/// space (GB) come from the filesystem view and are null when the disk has no mounted volumes.</summary>
public sealed record StorageDiskLoad(
    int DriveIndex,
    double ActivityPercent,
    double ReadRateMBps,
    double WriteRateMBps,
    double? ResponseMs,
    double? TemperatureC = null,
    double? HealthPercent = null,
    double? UsedSpacePercent = null,
    double? FreeSpaceGB = null,
    double? TotalSpaceGB = null,
    double? DataReadGB = null,
    double? DataWrittenGB = null,
    double? PowerOnHours = null,
    double? PowerOnCount = null,
    double ReadActivityPercent = 0,
    double WriteActivityPercent = 0);

/// <summary>A live storage reading: one <see cref="StorageDiskLoad"/> per physical disk.</summary>
public sealed record StorageLoadReading(IReadOnlyList<StorageDiskLoad> Disks);
