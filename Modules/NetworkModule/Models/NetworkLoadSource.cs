using System.Net.NetworkInformation;
using Crystal.Provider.Telemetry.Hardware;
using Crystal.Provider.Telemetry.Hardware.Network;

namespace NetworkModule.Models;

/// <summary>
/// Reads live per-interface network activity from the Telemetry provider (a LibreHardwareMonitor
/// fork). Every network interface exposes a "Network Utilization" <see cref="SensorType.Load"/>
/// sensor (clamped 0-100%) plus "Upload Speed"/"Download Speed" throughput sensors; we read those
/// per interface and key them by the interface name. For Wi-Fi adapters we additionally merge in
/// radio state (SSID, signal, band, channel, PHY type) from <see cref="IWlanSource"/>.
/// </summary>
public sealed class NetworkLoadSource : IDisposable {
  private const string UtilizationSensorName = "Network Utilization";
  private const string UploadSpeedSensorName = "Upload Speed";
  private const string DownloadSpeedSensorName = "Download Speed";

  private readonly Computer _computer;
  private readonly IWlanSource _wlan;
  private bool _disposed;

  public NetworkLoadSource(IWlanSource wlan) {
    ArgumentNullException.ThrowIfNull(wlan);
    _wlan = wlan;
    _computer = new Computer { IsNetworkEnabled = true };
    _computer.Open();
  }

  /// <summary>Re-samples every connected interface and returns one reading each.</summary>
  public IReadOnlyList<NetworkInterfaceReading> Read() {
    // The telemetry group enumerates every non-loopback/tunnel adapter — dozens of virtual and
    // disconnected NICs. Restrict to interfaces that are operationally up with a known link speed;
    // a down/virtual NIC reports Speed 0, so its utilization comes back NaN/Infinity.
    var connected = ConnectedInterfaceNames();

    // WLAN readings are keyed by interface GUID; the rest of the pipeline keys by friendly name, so
    // resolve GUID→name once and index the Wi-Fi data by name to merge it per adapter below.
    var wifiByName = ReadWifiByName();

    var readings = new List<NetworkInterfaceReading>();
    foreach (var nic in _computer.Hardware.Where(h => h.HardwareType == HardwareType.Network)) {
      if (!connected.Contains(nic.Name)) continue;
      nic.Update();
      wifiByName.TryGetValue(nic.Name, out var wifi);
      readings.Add(new NetworkInterfaceReading(
          Name: nic.Name,
          UtilizationPercent: Clamp(FindLoad(nic, UtilizationSensorName)),
          UploadBytesPerSecond: Sanitize(FindThroughput(nic, UploadSpeedSensorName)),
          DownloadBytesPerSecond: Sanitize(FindThroughput(nic, DownloadSpeedSensorName)),
          WifiSsid: wifi?.Ssid,
          WifiSignalPercent: wifi?.SignalQualityPercent,
          WifiRssiDbm: wifi?.RssiDbm,
          WifiPhyType: wifi?.PhyType,
          WifiChannel: wifi?.ChannelNumber,
          WifiBand: wifi?.Band,
          WifiRxRateKbps: wifi?.RxRateKbps,
          WifiTxRateKbps: wifi?.TxRateKbps,
          WifiBssid: wifi?.Bssid,
          WifiSecurity: wifi?.Security));
    }
    return readings;
  }

  // Joins WLAN readings (keyed by interface GUID) to the friendly interface name used elsewhere in
  // the pipeline. NetworkInterface.Id is the adapter's GUID string ("{XXXX-...}").
  private Dictionary<string, WlanReading> ReadWifiByName() {
    var wifi = _wlan.Read();
    if (wifi.Count == 0)
      return [];

    var guidToName = new Dictionary<Guid, string>();
    foreach (var nic in NetworkInterface.GetAllNetworkInterfaces()) {
      if (Guid.TryParse(nic.Id, out var guid))
        guidToName[guid] = nic.Name;
    }

    var byName = new Dictionary<string, WlanReading>(StringComparer.OrdinalIgnoreCase);
    foreach (var reading in wifi) {
      if (guidToName.TryGetValue(reading.InterfaceGuid, out var name))
        byName[name] = reading;
    }
    return byName;
  }

  private static HashSet<string> ConnectedInterfaceNames() {
    var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var nic in NetworkInterface.GetAllNetworkInterfaces()) {
      if (nic.OperationalStatus == OperationalStatus.Up && nic.Speed > 0)
        names.Add(nic.Name);
    }
    return names;
  }

  private static double FindLoad(IHardware nic, string name) {
    var sensor = Array.Find(nic.Sensors,
        s => s.SensorType == SensorType.Load
             && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
    return sensor?.Value ?? 0;
  }

  private static double FindThroughput(IHardware nic, string name) {
    var sensor = Array.Find(nic.Sensors,
        s => s.SensorType == SensorType.Throughput
             && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
    return sensor?.Value ?? 0;
  }

  private static double Clamp(double value) =>
      double.IsFinite(value) ? Math.Min(Math.Max(value, 0), 100) : 0;

  private static double Sanitize(double value) =>
      double.IsFinite(value) && value >= 0 ? value : 0;

  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _computer.Close();
  }
}
