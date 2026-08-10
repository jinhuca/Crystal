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
/// response time in milliseconds (null when the perf counter is unavailable).</summary>
public sealed record StorageDiskLoad(
    int DriveIndex,
    double ActivityPercent,
    double ReadRateMBps,
    double WriteRateMBps,
    double? ResponseMs);

/// <summary>A live storage reading: one <see cref="StorageDiskLoad"/> per physical disk.</summary>
public sealed record StorageLoadReading(IReadOnlyList<StorageDiskLoad> Disks);
