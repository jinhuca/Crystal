namespace NetworkModule.Models;

/// <summary>A live reading for one network interface: its utilization (0-100%) and current
/// throughput in bytes/second, keyed by <see cref="Name"/> so a consumer can correlate it
/// with the matching interface across polls.</summary>
public sealed record NetworkInterfaceReading(
    string Name,
    double UtilizationPercent,
    double UploadBytesPerSecond,
    double DownloadBytesPerSecond);

/// <summary>One poll of the network subsystem: a reading per connected interface.</summary>
public sealed record NetworkSnapshot(IReadOnlyList<NetworkInterfaceReading> Interfaces);
