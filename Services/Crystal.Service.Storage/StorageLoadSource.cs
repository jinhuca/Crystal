using System.Diagnostics;
using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Service.Storage;

/// <summary>
/// Reads live disk activity from the Telemetry provider (a LibreHardwareMonitor fork). Each
/// storage device exposes a "Total Activity" <see cref="SensorType.Load"/> sensor (percent of
/// time the disk was busy) plus "Read Rate" / "Write Rate" <see cref="SensorType.Throughput"/>
/// sensors in bytes/s. We report one reading per physical disk — keyed by the Windows physical-disk
/// number — mirroring Task Manager's per-disk Disk view. Average response time comes best-effort
/// from the "PhysicalDisk\Avg. Disk sec/Transfer" performance counter.
/// </summary>
public sealed class StorageLoadSource : IDisposable {
  private const string ActivitySensorName = "Total Activity";
  private const string ReadRateSensorName = "Read Rate";
  private const string WriteRateSensorName = "Write Rate";
  private const double BytesPerMB = 1024.0 * 1024.0;

  private readonly Computer _computer;
  // Per physical-disk-index average-response-time counters, created on first sight and reused.
  private readonly Dictionary<int, PerformanceCounter?> _responseCounters = new();
  private bool _disposed;

  public StorageLoadSource() {
    _computer = new Computer { IsStorageEnabled = true };
    _computer.Open();
  }

  /// <summary>Re-samples every physical disk and returns one <see cref="StorageDiskLoad"/> apiece:
  /// total-activity percentage, read/write rates (MB/s), and best-effort average response time.</summary>
  public StorageLoadReading Read() {
    var disks = new List<StorageDiskLoad>();
    foreach (var drive in _computer.Hardware.Where(h => h.HardwareType == HardwareType.Storage)) {
      drive.Update();

      if (DiskIndexOf(drive) is not { } index) continue;

      var activity = FindSensor(drive, SensorType.Load, ActivitySensorName)?.Value ?? 0;
      var read = FindSensor(drive, SensorType.Throughput, ReadRateSensorName)?.Value ?? 0;
      var write = FindSensor(drive, SensorType.Throughput, WriteRateSensorName)?.Value ?? 0;

      disks.Add(new StorageDiskLoad(
          DriveIndex: index,
          ActivityPercent: activity,
          ReadRateMBps: read / BytesPerMB,
          WriteRateMBps: write / BytesPerMB,
          ResponseMs: ReadResponseMs(index)));
    }
    return new StorageLoadReading(disks);
  }

  // The telemetry Identifier ends with the physical-disk number (StorageDeviceNumber), e.g.
  // "/nvme/0" -> 0. That's the same value as Win32_DiskDrive.Index, so it joins the two sources.
  private static int? DiskIndexOf(IHardware drive) {
    var token = drive.Identifier.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries)
        .LastOrDefault();
    return int.TryParse(token, out var index) ? index : null;
  }

  // Windows names PhysicalDisk instances by leading physical-disk index ("0 C:"), so match on the
  // token before the first space. Counters are cached (a null entry means "unavailable, don't retry").
  private double? ReadResponseMs(int index) {
    if (!_responseCounters.TryGetValue(index, out var counter)) {
      counter = CreateResponseCounter(index);
      _responseCounters[index] = counter;
    }
    if (counter is null) return null;
    try {
      // Counter is in seconds/transfer; surface milliseconds to match Task Manager.
      return counter.NextValue() * 1000.0;
    }
    catch {
      return null;
    }
  }

  private static PerformanceCounter? CreateResponseCounter(int index) {
    try {
      var category = new PerformanceCounterCategory("PhysicalDisk");
      var instance = Array.Find(category.GetInstanceNames(),
          name => name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() == index.ToString());
      if (instance is null) return null;
      var counter = new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Transfer", instance, readOnly: true);
      counter.NextValue(); // Prime; the first sample of a rate counter is always zero.
      return counter;
    }
    catch {
      return null;
    }
  }

  private static ISensor? FindSensor(IHardware drive, SensorType type, string name) =>
      Array.Find(drive.Sensors,
          s => s.SensorType == type && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    foreach (var counter in _responseCounters.Values) counter?.Dispose();
    _computer.Close();
  }
}
