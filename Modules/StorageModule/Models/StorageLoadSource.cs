using Crystal.Provider.Telemetry.Hardware;

namespace StorageModule.Models;

/// <summary>
/// Reads live disk activity from the Telemetry provider (a LibreHardwareMonitor fork). Each
/// storage device exposes a "Total Activity" <see cref="SensorType.Load"/> sensor (percent of
/// time the disk was busy) plus "Read Rate" / "Write Rate" <see cref="SensorType.Throughput"/>
/// sensors in bytes/s. We report the busiest drive's activity (matching a single CPU/GPU load
/// figure) alongside the system-wide transfer rate summed across all drives, mirroring Task
/// Manager's Disk view.
/// </summary>
public sealed class StorageLoadSource : IDisposable {
  private const string ActivitySensorName = "Total Activity";
  private const string ReadRateSensorName = "Read Rate";
  private const string WriteRateSensorName = "Write Rate";
  private const double BytesPerMB = 1024.0 * 1024.0;

  private readonly Computer _computer;
  private bool _disposed;

  public StorageLoadSource() {
    _computer = new Computer { IsStorageEnabled = true };
    _computer.Open();
  }

  /// <summary>Re-samples every drive and returns the busiest drive's total-activity percentage and
  /// the combined read+write transfer rate (MB/s across all drives).</summary>
  public StorageLoadReading Read() {
    double maxActivity = 0;
    double totalBytesPerSec = 0;
    foreach (var drive in _computer.Hardware.Where(h => h.HardwareType == HardwareType.Storage)) {
      drive.Update();

      var activity = FindSensor(drive, SensorType.Load, ActivitySensorName);
      if (activity?.Value is { } value && value > maxActivity) maxActivity = value;

      var read = FindSensor(drive, SensorType.Throughput, ReadRateSensorName);
      var write = FindSensor(drive, SensorType.Throughput, WriteRateSensorName);
      totalBytesPerSec += (read?.Value ?? 0) + (write?.Value ?? 0);
    }
    return new StorageLoadReading(maxActivity, totalBytesPerSec / BytesPerMB);
  }

  private static ISensor? FindSensor(IHardware drive, SensorType type, string name) =>
      Array.Find(drive.Sensors,
          s => s.SensorType == type && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _computer.Close();
  }
}
