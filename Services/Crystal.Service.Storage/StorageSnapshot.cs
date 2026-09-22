namespace Crystal.Service.Storage;

/// <summary>
/// One physical disk drive. <see cref="DriveIndex"/> is the Windows physical-disk number
/// (<c>Win32_DiskDrive.Index</c>, "Disk 0"/"Disk 1"/…) — the key that joins this static inventory
/// to the live per-disk telemetry readings. <see cref="IsSystemDisk"/> flags the disk hosting the
/// Windows/OS volume, which the UI selects by default.
/// </summary>
/// <param name="Model">The drive's model string (falls back to caption, else "Unknown drive").</param>
/// <param name="CapacityGB">Total capacity in GB, or null when WMI reports no size.</param>
/// <param name="InterfaceType">Bus/interface type (e.g. SCSI, IDE), or null when unknown.</param>
/// <param name="MediaType">Media type reported by WMI (e.g. fixed hard disk), or null when unknown.</param>
/// <param name="Manufacturer">Drive manufacturer, trimmed, or null when unreported.</param>
/// <param name="SerialNumber">Serial number, trimmed, or null when unreported.</param>
/// <param name="FirmwareRevision">Firmware revision, trimmed, or null when unreported.</param>
/// <param name="Partitions">Number of partitions on the drive, or null when unreported.</param>
/// <param name="DriveIndex">The Windows physical-disk number that joins to live telemetry.</param>
/// <param name="IsSystemDisk">True when this disk hosts the Windows/OS volume.</param>
public record StorageDriveInfo(
    string Model,
    double? CapacityGB,
    string? InterfaceType,
    string? MediaType,
    string? Manufacturer,
    string? SerialNumber,
    string? FirmwareRevision,
    uint? Partitions,
    int? DriveIndex,
    bool IsSystemDisk = false);

/// <summary>
/// The system's physical storage: the drives plus rolled-up totals.
/// </summary>
/// <param name="Drives">The physical drives in the inventory.</param>
/// <param name="TotalCapacityGB">Sum of the drives' capacities in GB (missing capacities count as 0).</param>
/// <param name="DriveCount">The number of drives in the inventory.</param>
public record StorageSnapshot(
    IReadOnlyList<StorageDriveInfo> Drives,
    double? TotalCapacityGB,
    int DriveCount);

/// <summary>
/// A single physical disk's live activity, matching Task Manager's per-disk Disk page:
/// total-activity percentage (0-100), read/write transfer rates in MB/s, and best-effort average
/// response time in milliseconds (null when the perf counter is unavailable). Temperature (°C) and
/// SSD health/"Life" (percent remaining) come from the drive's SMART sensors and are null when the
/// device doesn't report them or elevation/PawnIO isn't available. Used-space percent and free/total
/// space (GB) come from the filesystem view and are null when the disk has no mounted volumes.
/// </summary>
/// <param name="DriveIndex">The Windows physical-disk number this reading belongs to.</param>
/// <param name="ActivityPercent">Total time the disk was busy, 0-100.</param>
/// <param name="ReadRateMBps">Read throughput in MB/s.</param>
/// <param name="WriteRateMBps">Write throughput in MB/s.</param>
/// <param name="ResponseMs">Average response time in milliseconds, or null when the counter is unavailable.</param>
/// <param name="TemperatureC">SMART temperature in °C, or null when unreported.</param>
/// <param name="HealthPercent">SSD remaining life in percent, or null when unreported.</param>
/// <param name="UsedSpacePercent">Used space as a percent of capacity, or null when no volume is mounted.</param>
/// <param name="FreeSpaceGB">Free space in GB, or null when no volume is mounted.</param>
/// <param name="TotalSpaceGB">Total filesystem space in GB, or null when no volume is mounted.</param>
/// <param name="DataReadGB">Lifetime data read in GB, or null when unreported.</param>
/// <param name="DataWrittenGB">Lifetime data written in GB, or null when unreported.</param>
/// <param name="PowerOnHours">SMART power-on hours, or null when unreported.</param>
/// <param name="PowerOnCount">SMART power-on (power-cycle) count, or null when unreported.</param>
/// <param name="ReadActivityPercent">Time servicing reads, 0-100.</param>
/// <param name="WriteActivityPercent">Time servicing writes, 0-100.</param>
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

/// <summary>
/// A live storage reading: one <see cref="StorageDiskLoad"/> per physical disk.
/// </summary>
/// <param name="Disks">The per-physical-disk activity readings for this sample.</param>
public sealed record StorageLoadReading(IReadOnlyList<StorageDiskLoad> Disks);
