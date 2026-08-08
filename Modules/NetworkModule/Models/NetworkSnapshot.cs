namespace NetworkModule.Models;

/// <summary>A live reading for one network interface: its utilization (0-100%) and current
/// throughput in bytes/second, keyed by <see cref="Name"/> so a consumer can correlate it
/// with the matching interface across polls. The <c>Wifi*</c> fields are populated only for a
/// connected Wi-Fi adapter and are null for wired/virtual NICs (rendered as "—").</summary>
public sealed record NetworkInterfaceReading(
    string Name,
    double UtilizationPercent,
    double UploadBytesPerSecond,
    double DownloadBytesPerSecond,
    string? WifiSsid = null,
    int? WifiSignalPercent = null,
    int? WifiRssiDbm = null,
    string? WifiPhyType = null,
    int? WifiChannel = null,
    string? WifiBand = null,
    int? WifiRxRateKbps = null,
    int? WifiTxRateKbps = null,
    string? WifiBssid = null,
    string? WifiSecurity = null);

/// <summary>One poll of the network subsystem: a reading per connected interface.</summary>
public sealed record NetworkSnapshot(IReadOnlyList<NetworkInterfaceReading> Interfaces);
