using System.Net.NetworkInformation;
using Crystal.Provider.Telemetry.Hardware;

namespace NetworkModule.Models;

/// <summary>
/// Reads live per-interface network activity from the Telemetry provider (a LibreHardwareMonitor
/// fork). Every network interface exposes a "Network Utilization" <see cref="SensorType.Load"/>
/// sensor (clamped 0-100%) plus "Upload Speed"/"Download Speed" throughput sensors; we read those
/// per interface and key them by the interface name.
/// </summary>
public sealed class NetworkLoadSource : IDisposable {
  private const string UtilizationSensorName = "Network Utilization";
  private const string UploadSpeedSensorName = "Upload Speed";
  private const string DownloadSpeedSensorName = "Download Speed";

  private readonly Computer _computer;
  private bool _disposed;

  public NetworkLoadSource() {
    _computer = new Computer { IsNetworkEnabled = true };
    _computer.Open();
  }

  /// <summary>Re-samples every connected interface and returns one reading each.</summary>
  public IReadOnlyList<NetworkInterfaceReading> Read() {
    // The telemetry group enumerates every non-loopback/tunnel adapter — dozens of virtual and
    // disconnected NICs. Restrict to interfaces that are operationally up with a known link speed;
    // a down/virtual NIC reports Speed 0, so its utilization comes back NaN/Infinity.
    var connected = ConnectedInterfaceNames();

    var readings = new List<NetworkInterfaceReading>();
    foreach (var nic in _computer.Hardware.Where(h => h.HardwareType == HardwareType.Network)) {
      if (!connected.Contains(nic.Name)) continue;
      nic.Update();
      readings.Add(new NetworkInterfaceReading(
          Name: nic.Name,
          UtilizationPercent: Clamp(FindLoad(nic, UtilizationSensorName)),
          UploadBytesPerSecond: Sanitize(FindThroughput(nic, UploadSpeedSensorName)),
          DownloadBytesPerSecond: Sanitize(FindThroughput(nic, DownloadSpeedSensorName))));
    }
    return readings;
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
