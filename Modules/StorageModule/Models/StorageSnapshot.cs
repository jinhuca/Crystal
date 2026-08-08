namespace StorageModule.Models;

/// <summary>One physical disk drive.</summary>
public record StorageDriveInfo(
    string Model,
    double? CapacityGB,
    string? InterfaceType,
    string? MediaType,
    string? Manufacturer,
    string? SerialNumber,
    string? FirmwareRevision,
    uint? Partitions);

/// <summary>The system's physical storage: the drives plus rolled-up totals.</summary>
public record StorageSnapshot(
    IReadOnlyList<StorageDriveInfo> Drives,
    double? TotalCapacityGB,
    int DriveCount);

/// <summary>A live storage reading: the busiest drive's total-activity percentage (0-100) and the
/// system-wide transfer rate in MB/s (read + write summed across all drives).</summary>
public sealed record StorageLoadReading(double ActivityPercent, double TransferRateMBps);
