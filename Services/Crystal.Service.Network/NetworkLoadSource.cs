using Crystal.Provider.Telemetry.Hardware;
using Crystal.Provider.Telemetry.Hardware.Network;
using System.Net.NetworkInformation;

namespace Crystal.Service.Network;

/// <summary>
/// Reads live per-interface network activity from the Telemetry provider (a LibreHardwareMonitor
/// fork). Every network interface exposes a "Network Utilization" <see cref="SensorType.Load"/>
/// sensor (clamped 0-100%), "Upload Speed"/"Download Speed" throughput sensors, and cumulative
/// "Data Uploaded"/"Data Downloaded" counters (GB); we read those per interface, key them by name,
/// and capture the OS-reported link speed. For Wi-Fi adapters we additionally merge in radio state
/// (SSID, signal, band, channel, PHY type) from <see cref="IWlanSource"/>.
/// </summary>
public sealed class NetworkLoadSource : INetworkLoadSource, IDisposable {
  /// <summary>
  /// Provider sensor name for per-interface utilization (a <see cref="SensorType.Load"/>, 0-100%).
  /// </summary>
  private const string UtilizationSensorName = "Network Utilization";

  /// <summary>
  /// Provider sensor name for outbound throughput (a <see cref="SensorType.Throughput"/>, bytes/sec).
  /// </summary>
  private const string UploadSpeedSensorName = "Upload Speed";

  /// <summary>
  /// Provider sensor name for inbound throughput (a <see cref="SensorType.Throughput"/>, bytes/sec).
  /// </summary>
  private const string DownloadSpeedSensorName = "Download Speed";

  /// <summary>
  /// Provider sensor name for the cumulative bytes-sent counter (a <see cref="SensorType.Data"/>, GB).
  /// </summary>
  private const string DataUploadedSensorName = "Data Uploaded";

  /// <summary>
  /// Provider sensor name for the cumulative bytes-received counter (a <see cref="SensorType.Data"/>, GB).
  /// </summary>
  private const string DataDownloadedSensorName = "Data Downloaded";

  /// <summary>
  /// The LibreHardwareMonitor computer handle, opened with only network hardware enabled.
  /// </summary>
  private readonly Computer _computer;

  /// <summary>
  /// Machine-level Wi-Fi radio source, merged in per adapter to enrich wireless interfaces.
  /// </summary>
  private readonly IWlanSource _wlan;

  /// <summary>
  /// Guards <see cref="Dispose"/> so the underlying <see cref="Computer"/> is closed only once.
  /// </summary>
  private bool _disposed;

  /// <summary>
  /// Opens the underlying hardware <see cref="Computer"/> (network sensors only) and captures the
  /// Wi-Fi source. Opening hardware here is why the interface is faked in tests rather than this type.
  /// </summary>
  /// <param name="wlan">Source of per-radio WLAN state used to enrich Wi-Fi interface readings.</param>
  public NetworkLoadSource(IWlanSource wlan) {
    ArgumentNullException.ThrowIfNull(wlan);
    _wlan = wlan;
    _computer = new Computer { IsNetworkEnabled = true };
    _computer.Open();
  }

  /// <summary>
  /// Re-samples every connected interface plus the machine-level Wi-Fi state.
  /// </summary>
  /// <returns>A snapshot of the current per-interface readings and overall Wi-Fi status.</returns>
  public NetworkSnapshot Read() {
    // The telemetry group enumerates every non-loopback/tunnel adapter — dozens of virtual and
    // disconnected NICs. Restrict to interfaces that are operationally up with a known link speed;
    // a down/virtual NIC reports Speed 0, so its utilization comes back NaN/Infinity.
    var linkSpeeds = ConnectedInterfaceSpeeds();

    // One WLAN reading per present radio (connected or not). Keyed by GUID; the rest of the
    // pipeline keys by friendly name, so index the connected ones by name to merge per adapter.
    var wlan = _wlan.Read();
    var wifiByName = IndexConnectedByName(wlan);

    var readings = new List<NetworkInterfaceReading>();
    foreach (var nic in _computer.Hardware.Where(h => h.HardwareType == HardwareType.Network)) {
      if (!linkSpeeds.TryGetValue(nic.Name, out var linkSpeed)) continue;
      nic.Update();
      wifiByName.TryGetValue(nic.Name, out var wifi);
      readings.Add(new NetworkInterfaceReading(
          Name: nic.Name,
          UtilizationPercent: NetworkSensorSelector.Clamp(
              NetworkSensorSelector.FindValue(nic.Sensors, SensorType.Load, UtilizationSensorName)),
          UploadBytesPerSecond: NetworkSensorSelector.Sanitize(
              NetworkSensorSelector.FindValue(nic.Sensors, SensorType.Throughput, UploadSpeedSensorName)),
          DownloadBytesPerSecond: NetworkSensorSelector.Sanitize(
              NetworkSensorSelector.FindValue(nic.Sensors, SensorType.Throughput, DownloadSpeedSensorName)),
          WifiSsid: wifi?.Ssid,
          WifiSignalPercent: wifi?.SignalQualityPercent,
          WifiRssiDbm: wifi?.RssiDbm,
          WifiPhyType: wifi?.PhyType,
          WifiChannel: wifi?.ChannelNumber,
          WifiBand: wifi?.Band,
          WifiRxRateKbps: wifi?.RxRateKbps,
          WifiTxRateKbps: wifi?.TxRateKbps,
          WifiBssid: wifi?.Bssid,
          WifiSecurity: wifi?.Security,
          DataUploadedGb: NetworkSensorSelector.Sanitize(
              NetworkSensorSelector.FindValue(nic.Sensors, SensorType.Data, DataUploadedSensorName)),
          DataDownloadedGb: NetworkSensorSelector.Sanitize(
              NetworkSensorSelector.FindValue(nic.Sensors, SensorType.Data, DataDownloadedSensorName)),
          LinkSpeedBitsPerSecond: linkSpeed));
    }
    return new NetworkSnapshot(readings, ComputeWifiStatus(wlan));
  }

  /// <summary>
  /// Reduces every present radio's state to a single machine-level status, best-state-wins
  /// (Connected beats Disconnected beats Disabled). When the WLAN service enumerates nothing we
  /// fall back to the OS adapter list so a driver-managed radio the service can't see still reads
  /// as present (<see cref="WifiStatus.Disabled"/>) rather than <see cref="WifiStatus.None"/>.
  /// </summary>
  private static WifiStatus ComputeWifiStatus(IReadOnlyList<WlanReading> wlan) {
    if (wlan.Count == 0)
      return HasWirelessAdapter() ? WifiStatus.Disabled : WifiStatus.None;

    return NetworkSensorSelector.ReduceWifiStatus([.. wlan.Select(w => w.State)]);
  }

  /// <summary>
  /// Probes the OS adapter list for any 802.11 interface. Used to distinguish a machine with a
  /// present-but-off radio (<see cref="WifiStatus.Disabled"/>) from one with no radio at all.
  /// </summary>
  private static bool HasWirelessAdapter() {
    foreach (var nic in NetworkInterface.GetAllNetworkInterfaces()) {
      if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
        return true;
    }
    return false;
  }

  /// <summary>
  /// Joins connected WLAN readings (keyed by interface GUID) to the friendly interface name used
  /// elsewhere in the pipeline, so per-radio Wi-Fi state can be merged onto the matching NIC.
  /// <c>NetworkInterface.Id</c> is the adapter's GUID string ("{XXXX-...}").
  /// </summary>
  private static Dictionary<string, WlanReading> IndexConnectedByName(IReadOnlyList<WlanReading> wlan) {
    var byName = new Dictionary<string, WlanReading>(StringComparer.OrdinalIgnoreCase);
    if (wlan.Count == 0)
      return byName;

    var guidToName = new Dictionary<Guid, string>();
    foreach (var nic in NetworkInterface.GetAllNetworkInterfaces()) {
      if (Guid.TryParse(nic.Id, out var guid))
        guidToName[guid] = nic.Name;
    }

    foreach (var reading in wlan) {
      if (reading.State != WlanInterfaceState.Connected) continue;
      if (guidToName.TryGetValue(reading.InterfaceGuid, out var name))
        byName[name] = reading;
    }
    return byName;
  }

  /// <summary>
  /// Maps each connected interface's friendly name to its link speed (bits/sec). Doubles as the
  /// connected-interface filter: a down/virtual NIC reports Speed 0, so its utilization comes back
  /// NaN/Infinity — only interfaces present in this map are read.
  /// </summary>
  private static Dictionary<string, long> ConnectedInterfaceSpeeds() {
    var speeds = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
    foreach (var nic in NetworkInterface.GetAllNetworkInterfaces()) {
      if (nic.OperationalStatus == OperationalStatus.Up && nic.Speed > 0)
        speeds[nic.Name] = nic.Speed;
    }
    return speeds;
  }

  /// <summary>
  /// Closes the underlying hardware <see cref="Computer"/> handle. Idempotent.
  /// </summary>
  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _computer.Close();
  }
}
