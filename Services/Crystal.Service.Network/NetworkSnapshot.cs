namespace Crystal.Service.Network;

/// <summary>
/// A live reading for one network interface: its utilization (0-100%) and current
/// throughput in bytes/second, keyed by <see cref="Name"/> so a consumer can correlate it
/// with the matching interface across polls. The <c>Wifi*</c> fields are populated only for a
/// connected Wi-Fi adapter and are null for wired/virtual NICs (rendered as "—").
/// </summary>
/// <param name="Name">Friendly interface name; the correlation key across polls.</param>
/// <param name="UtilizationPercent">Current link utilization, 0-100%.</param>
/// <param name="UploadBytesPerSecond">Instantaneous outbound throughput in bytes/second.</param>
/// <param name="DownloadBytesPerSecond">Instantaneous inbound throughput in bytes/second.</param>
/// <param name="WifiSsid">Associated network name; null for non-Wi-Fi or unassociated adapters.</param>
/// <param name="WifiSignalPercent">Signal quality, 0-100%; null when not applicable.</param>
/// <param name="WifiRssiDbm">Received signal strength in dBm (negative, higher is stronger); null when not applicable.</param>
/// <param name="WifiPhyType">802.11 PHY type (e.g. "802.11ac"); null when not applicable.</param>
/// <param name="WifiChannel">Wireless channel number; null when not applicable.</param>
/// <param name="WifiBand">Frequency band (e.g. "5 GHz"); null when not applicable.</param>
/// <param name="WifiRxRateKbps">Negotiated receive rate in kbps; null when not applicable.</param>
/// <param name="WifiTxRateKbps">Negotiated transmit rate in kbps; null when not applicable.</param>
/// <param name="WifiBssid">Access point MAC address; null when not applicable.</param>
/// <param name="WifiSecurity">Security/authentication scheme (e.g. "WPA2"); null when not applicable.</param>
/// <param name="DataUploadedGb">Cumulative bytes sent since counter start, in GB.</param>
/// <param name="DataDownloadedGb">Cumulative bytes received since counter start, in GB.</param>
/// <param name="LinkSpeedBitsPerSecond">OS-reported negotiated link speed in bits/second.</param>
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

/// <summary>
/// Machine-level Wi-Fi availability, independent of any single interface reading. Lets the
/// UI distinguish "no wireless radio at all" (hide everything) from a present-but-off or
/// present-but-unassociated radio (show a muted status).
/// </summary>
public enum WifiStatus {
  /// <summary>
  /// No WLAN interface exists on the machine (typical desktop).
  /// </summary>
  None,

  /// <summary>
  /// A wireless radio exists but is off/disabled (airplane mode, adapter disabled).
  /// </summary>
  Disabled,

  /// <summary>
  /// A wireless radio is on but not associated to any access point.
  /// </summary>
  Disconnected,

  /// <summary>
  /// At least one wireless radio is associated to an access point.
  /// </summary>
  Connected,
}

/// <summary>
/// One poll of the network subsystem: a reading per connected interface plus the overall
/// Wi-Fi availability state.
/// </summary>
/// <param name="Interfaces">One reading per connected interface at this poll.</param>
/// <param name="WifiStatus">Machine-level Wi-Fi availability, reduced across all radios.</param>
public sealed record NetworkSnapshot(
    IReadOnlyList<NetworkInterfaceReading> Interfaces,
    WifiStatus WifiStatus = WifiStatus.None);
