using Crystal.Provider.Telemetry.Hardware;
using Crystal.Provider.Telemetry.Hardware.Network;

namespace Crystal.Service.Network;

/// <summary>
/// Pure sensor-selection and value-sanitizing logic for network telemetry, split out from
/// <see cref="NetworkLoadSource"/> so it can be unit-tested without opening a hardware
/// <c>Computer</c> or calling the OS network stack. The source layer keeps only the
/// Update()/enumeration and <see cref="System.Net.NetworkInformation"/> side effects.
/// </summary>
internal static class NetworkSensorSelector {
  public static double FindValue(ISensor[] sensors, SensorType type, string name) {
    var sensor = Array.Find(sensors,
        s => s.SensorType == type && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
    return sensor?.Value ?? 0;
  }

  // Utilization is a percentage; a down/virtual NIC can report NaN/Infinity (Speed 0 → divide),
  // so clamp non-finite values to 0 and cap the range at 0-100.
  public static double Clamp(double value) =>
      double.IsFinite(value) ? Math.Min(Math.Max(value, 0), 100) : 0;

  // Throughput/data counters must be finite and non-negative; anything else reads as 0.
  public static double Sanitize(double value) =>
      double.IsFinite(value) && value >= 0 ? value : 0;

  // Reduces every present radio's state to a single machine-level status, best-state-wins
  // (Connected beats Disconnected beats Disabled). Callers decide the empty-list fallback
  // (None vs Disabled) since that needs an OS adapter probe.
  public static WifiStatus ReduceWifiStatus(IReadOnlyList<WlanInterfaceState> states) {
    var best = WifiStatus.Disabled;
    foreach (var state in states) {
      var status = state switch {
        WlanInterfaceState.Connected => WifiStatus.Connected,
        WlanInterfaceState.Disconnected => WifiStatus.Disconnected,
        _ => WifiStatus.Disabled,
      };
      if (status > best) best = status;
    }
    return best;
  }
}
