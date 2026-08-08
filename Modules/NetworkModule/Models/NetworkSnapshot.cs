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
    string? WifiSecurity = null,
    double DataUploadedGb = 0,
    double DataDownloadedGb = 0,
    long LinkSpeedBitsPerSecond = 0);

/// <summary>Machine-level Wi-Fi availability, independent of any single interface reading. Lets the
/// UI distinguish "no wireless radio at all" (hide everything) from a present-but-off or
/// present-but-unassociated radio (show a muted status).</summary>
public enum WifiStatus {
  /// <summary>No WLAN interface exists on the machine (typical desktop).</summary>
  None,

  /// <summary>A wireless radio exists but is off/disabled (airplane mode, adapter disabled).</summary>
  Disabled,

  /// <summary>A wireless radio is on but not associated to any access point.</summary>
  Disconnected,

  /// <summary>At least one wireless radio is associated to an access point.</summary>
  Connected,
}

/// <summary>One poll of the network subsystem: a reading per connected interface plus the overall
/// Wi-Fi availability state.</summary>
public sealed record NetworkSnapshot(
    IReadOnlyList<NetworkInterfaceReading> Interfaces,
    WifiStatus WifiStatus = WifiStatus.None);
