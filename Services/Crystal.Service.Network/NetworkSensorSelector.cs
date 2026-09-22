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
  /// <summary>
  /// Finds the first sensor matching both <paramref name="type"/> and <paramref name="name"/>
  /// (case-insensitive) and returns its value, or 0 when the sensor is absent or unread. The name
  /// match is needed because one interface exposes several sensors of the same <see cref="SensorType"/>.
  /// </summary>
  /// <param name="sensors">The interface's sensor array to search.</param>
  /// <param name="type">The sensor type to match (e.g. <see cref="SensorType.Throughput"/>).</param>
  /// <param name="name">The sensor's display name to match.</param>
  /// <returns>The sensor's current value, or 0 if not found.</returns>
  public static double FindValue(ISensor[] sensors, SensorType type, string name) {
    var sensor = Array.Find(sensors,
        s => s.SensorType == type && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
    return sensor?.Value ?? 0;
  }

  /// <summary>
  /// Coerces a utilization reading into a valid percentage. A down/virtual NIC can report
  /// NaN/Infinity (Speed 0 → divide), so non-finite values become 0 and the result is capped to 0-100.
  /// </summary>
  public static double Clamp(double value) =>
      double.IsFinite(value) ? Math.Min(Math.Max(value, 0), 100) : 0;

  /// <summary>
  /// Coerces a throughput or cumulative-data reading to a usable number: finite and non-negative
  /// values pass through, anything else (NaN, Infinity, negative) reads as 0.
  /// </summary>
  public static double Sanitize(double value) =>
      double.IsFinite(value) && value >= 0 ? value : 0;

  /// <summary>
  /// Reduces every present radio's state to a single machine-level status, best-state-wins
  /// (Connected beats Disconnected beats Disabled). Callers decide the empty-list fallback
  /// (<see cref="WifiStatus.None"/> vs <see cref="WifiStatus.Disabled"/>) since that needs an OS adapter probe.
  /// </summary>
  /// <param name="states">Per-radio WLAN interface states to reduce.</param>
  /// <returns>The best (highest) <see cref="WifiStatus"/> across the given radios.</returns>
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
