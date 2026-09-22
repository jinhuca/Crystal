using Crystal.Provider.Telemetry.Hardware;
using System.Diagnostics;

namespace Crystal.Service.Storage;

/// <summary>
/// Reads live disk activity from the Telemetry provider (a LibreHardwareMonitor fork). Each
/// storage device exposes a "Total Activity" <see cref="SensorType.Load"/> sensor (percent of
/// time the disk was busy) plus "Read Rate" / "Write Rate" <see cref="SensorType.Throughput"/>
/// sensors in bytes/s. We report one reading per physical disk — keyed by the Windows physical-disk
/// number — mirroring Task Manager's per-disk Disk view. Average response time comes best-effort
/// from the "PhysicalDisk\Avg. Disk sec/Transfer" performance counter.
/// </summary>
public sealed class StorageLoadSource : IStorageLoadSource, IDisposable {
  // The exact sensor names exposed by the Telemetry provider per storage device. They are matched
  // case-insensitively via StorageSensorSelector.FindSensor and must track the provider's naming.

  /// <summary>
  /// Name of the "percent of time the disk was busy" load sensor.
  /// </summary>
  private const string ActivitySensorName = "Total Activity";

  /// <summary>
  /// Name of the read-busy load sensor (percent of time servicing reads).
  /// </summary>
  private const string ReadActivitySensorName = "Read Activity";

  /// <summary>
  /// Name of the write-busy load sensor (percent of time servicing writes).
  /// </summary>
  private const string WriteActivitySensorName = "Write Activity";

  /// <summary>
  /// Name of the read-throughput sensor, reported in bytes/second.
  /// </summary>
  private const string ReadRateSensorName = "Read Rate";

  /// <summary>
  /// Name of the write-throughput sensor, reported in bytes/second.
  /// </summary>
  private const string WriteRateSensorName = "Write Rate";

  /// <summary>
  /// Name of the SMART temperature sensor, in degrees Celsius.
  /// </summary>
  private const string TemperatureSensorName = "Temperature";

  /// <summary>
  /// Name of the SSD remaining-life ("Life") level sensor, in percent.
  /// </summary>
  private const string HealthSensorName = "Life";

  /// <summary>
  /// Name of the used-space load sensor, in percent of capacity.
  /// </summary>
  private const string UsedSpaceSensorName = "Used Space";

  /// <summary>
  /// Name of the free-space data sensor, in gigabytes.
  /// </summary>
  private const string FreeSpaceSensorName = "Free Space";

  /// <summary>
  /// Name of the total-space data sensor, in gigabytes.
  /// </summary>
  private const string TotalSpaceSensorName = "Total Space";

  /// <summary>
  /// Name of the lifetime data-read counter sensor, in gigabytes.
  /// </summary>
  private const string DataReadSensorName = "Data Read";

  /// <summary>
  /// Name of the lifetime data-written counter sensor, in gigabytes.
  /// </summary>
  private const string DataWrittenSensorName = "Data Written";

  /// <summary>
  /// Name of the SMART power-on-hours counter sensor.
  /// </summary>
  private const string PowerOnHoursSensorName = "Power On Hours";

  /// <summary>
  /// Name of the SMART power-on-count (power cycles) counter sensor.
  /// </summary>
  private const string PowerOnCountSensorName = "Power On Count";

  /// <summary>
  /// The LibreHardwareMonitor hardware session, opened once with storage enabled and reused
  /// for every poll. Rebuilt in-place by <see cref="Refresh"/> when the physical-drive set changes.
  /// </summary>
  private readonly Computer _computer;
  // Per physical-disk-index average-response-time counters, created on first sight and reused.
  private readonly Dictionary<int, PerformanceCounter?> _responseCounters = new();
  // Serializes Read() against Refresh()/Dispose(): a Refresh tears the hardware group down and
  // rebuilds it (Computer.Reset), which must not overlap a concurrent enumeration on the poll thread.
  private readonly object _gate = new();

  /// <summary>
  /// Set once by <see cref="Dispose"/>; a disposed source returns an empty reading and
  /// makes <see cref="Refresh"/> a no-op rather than touching the closed hardware session.
  /// </summary>
  private bool _disposed;

  /// <summary>
  /// Opens the storage hardware session eagerly so the first <see cref="Read"/> already has
  /// the current drive set. The session is kept open for the lifetime of the source.
  /// </summary>
  public StorageLoadSource() {
    _computer = new Computer { IsStorageEnabled = true };
    _computer.Open();
  }

  /// <summary>
  /// Re-samples every physical disk and returns one <see cref="StorageDiskLoad"/> apiece:
  /// total-activity percentage, read/write rates (MB/s), and best-effort average response time.
  /// </summary>
  public StorageLoadReading Read() {
    lock (_gate) {
      if (_disposed) return new StorageLoadReading([]);
      return ReadLocked();
    }
  }

  /// <summary>
  /// Enumerates the storage hardware under <see cref="_gate"/>, calling <c>Update()</c> to
  /// refresh each drive's sensors, then pulls the named sensors into a <see cref="StorageDiskLoad"/>.
  /// Drives whose identifier can't be correlated to a physical-disk number are skipped, and any sensor
  /// the device doesn't expose falls back to null (or 0 for the always-present activity/rate values).
  /// </summary>
  private StorageLoadReading ReadLocked() {
    var disks = new List<StorageDiskLoad>();
    foreach (var drive in _computer.Hardware.Where(h => h.HardwareType == HardwareType.Storage)) {
      drive.Update();

      if (StorageSensorSelector.DiskIndexOf(drive.Identifier) is not { } index) continue;

      var activity = StorageSensorSelector.FindSensor(drive.Sensors, SensorType.Load, ActivitySensorName)?.Value ?? 0;
      var readActivity = StorageSensorSelector.FindSensor(drive.Sensors, SensorType.Load, ReadActivitySensorName)?.Value ?? 0;
      var writeActivity = StorageSensorSelector.FindSensor(drive.Sensors, SensorType.Load, WriteActivitySensorName)?.Value ?? 0;
      var read = StorageSensorSelector.FindSensor(drive.Sensors, SensorType.Throughput, ReadRateSensorName)?.Value ?? 0;
      var write = StorageSensorSelector.FindSensor(drive.Sensors, SensorType.Throughput, WriteRateSensorName)?.Value ?? 0;
      var temperature = StorageSensorSelector.FindSensor(drive.Sensors, SensorType.Temperature, TemperatureSensorName)?.Value;
      var health = StorageSensorSelector.FindSensor(drive.Sensors, SensorType.Level, HealthSensorName)?.Value;
      var usedSpace = StorageSensorSelector.FindSensor(drive.Sensors, SensorType.Load, UsedSpaceSensorName)?.Value;
      var freeSpace = StorageSensorSelector.FindSensor(drive.Sensors, SensorType.Data, FreeSpaceSensorName)?.Value;
      var totalSpace = StorageSensorSelector.FindSensor(drive.Sensors, SensorType.Data, TotalSpaceSensorName)?.Value;
      var dataRead = StorageSensorSelector.FindSensor(drive.Sensors, SensorType.Data, DataReadSensorName)?.Value;
      var dataWritten = StorageSensorSelector.FindSensor(drive.Sensors, SensorType.Data, DataWrittenSensorName)?.Value;
      var powerOnHours = StorageSensorSelector.FindSensor(drive.Sensors, SensorType.Factor, PowerOnHoursSensorName)?.Value;
      var powerOnCount = StorageSensorSelector.FindSensor(drive.Sensors, SensorType.Factor, PowerOnCountSensorName)?.Value;

      disks.Add(new StorageDiskLoad(
          DriveIndex: index,
          ActivityPercent: activity,
          ReadRateMBps: StorageSensorSelector.BytesToMBps(read),
          WriteRateMBps: StorageSensorSelector.BytesToMBps(write),
          ResponseMs: ReadResponseMs(index),
          TemperatureC: temperature,
          HealthPercent: health,
          UsedSpacePercent: usedSpace,
          FreeSpaceGB: freeSpace,
          TotalSpaceGB: totalSpace,
          DataReadGB: dataRead,
          DataWrittenGB: dataWritten,
          PowerOnHours: powerOnHours,
          PowerOnCount: powerOnCount,
          ReadActivityPercent: readActivity,
          WriteActivityPercent: writeActivity));
    }
    return new StorageLoadReading(disks);
  }

  /// <summary>
  /// Returns the disk's average response time in milliseconds from its cached PhysicalDisk performance
  /// counter, or null when the counter is unavailable.
  /// </summary>
  /// <remarks>
  /// Windows names PhysicalDisk instances by leading physical-disk index ("0 C:"), so match on the
  /// token before the first space. Counters are cached (a null entry means "unavailable, don't retry").
  /// </remarks>
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

  /// <summary>
  /// Locates the PhysicalDisk perf-counter instance whose leading token is the given
  /// physical-disk index and returns a primed "Avg. Disk sec/Transfer" counter for it, or null when no
  /// matching instance exists or the counters are unavailable (so the caller stops retrying).
  /// </summary>
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

  /// <summary>
  /// Rebuilds the hardware group so a newly attached disk starts reporting and a removed one
  /// drops out; LibreHardwareMonitor only picks up physical-drive changes on a re-scan, not on
  /// <c>Update()</c>. Response-time counters are dropped so a re-plugged disk at the same index gets
  /// a fresh one instead of a stale handle.
  /// </summary>
  public void Refresh() {
    lock (_gate) {
      if (_disposed) return;
      _computer.Reset();
      foreach (var counter in _responseCounters.Values) counter?.Dispose();
      _responseCounters.Clear();
    }
  }

  /// <summary>
  /// Closes the hardware session and disposes every cached response-time counter. Guarded by
  /// <see cref="_gate"/> so it can't race an in-flight <see cref="Read"/>; idempotent once disposed.
  /// </summary>
  public void Dispose() {
    lock (_gate) {
      if (_disposed) return;
      _disposed = true;
      foreach (var counter in _responseCounters.Values) counter?.Dispose();
      _computer.Close();
    }
  }
}
