using Crystal.Provider.Telemetry.Hardware;

namespace StorageModule.Models;

/// <summary>
/// Reads live disk activity from the Telemetry provider (a LibreHardwareMonitor fork). Each
/// storage device exposes a "Total Activity" <see cref="SensorType.Load"/> sensor (percent of
/// time the disk was busy). We report the busiest drive so the dashboard gauge reflects overall
/// storage pressure, matching how a single CPU/GPU load figure is shown.
/// </summary>
public sealed class StorageLoadSource : IDisposable {
  private const string ActivitySensorName = "Total Activity";

  private readonly Computer _computer;
  private bool _disposed;

  public StorageLoadSource() {
    _computer = new Computer { IsStorageEnabled = true };
    _computer.Open();
  }

  /// <summary>Re-samples every drive and returns the highest total-activity percentage.</summary>
  public double Read() {
    double max = 0;
    foreach (var drive in _computer.Hardware.Where(h => h.HardwareType == HardwareType.Storage)) {
      drive.Update();
      var activity = Array.Find(drive.Sensors,
          s => s.SensorType == SensorType.Load
               && string.Equals(s.Name, ActivitySensorName, StringComparison.OrdinalIgnoreCase));
      if (activity?.Value is { } value && value > max) max = value;
    }
    return max;
  }

  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _computer.Close();
  }
}
