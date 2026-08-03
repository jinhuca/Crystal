namespace StorageModule.Models;

/// <summary>One physical disk drive.</summary>
public record StorageDriveInfo(
    string Model,
    double? CapacityGB,
    string? InterfaceType,
    string? MediaType,
    string? SerialNumber,
    string? FirmwareRevision,
    uint? Partitions);

/// <summary>The system's physical storage: the drives plus rolled-up totals.</summary>
public record StorageSnapshot(
    IReadOnlyList<StorageDriveInfo> Drives,
    double? TotalCapacityGB,
    int DriveCount);
