using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Service.Storage;

/// <summary>
/// Pure sensor/identifier logic for a storage device's telemetry data, split out from
/// <see cref="StorageLoadSource"/> so it can be unit-tested without opening a hardware
/// <c>Computer</c> or touching performance counters. The source layer keeps only the
/// Update()/enumeration and perf-counter side effects.
/// </summary>
internal static class StorageSensorSelector {
  private const double BytesPerMB = 1024.0 * 1024.0;

  // The telemetry Identifier ends with the physical-disk number (StorageDeviceNumber), e.g.
  // "/nvme/0" -> 0. That's the same value as Win32_DiskDrive.Index, so it joins the two sources.
  // A trailing token that isn't an integer means the disk can't be correlated → null (skip it).
  public static int? DiskIndexOf(Identifier identifier) {
    var token = identifier.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries)
        .LastOrDefault();
    return int.TryParse(token, out var index) ? index : null;
  }

  public static ISensor? FindSensor(ISensor[] sensors, SensorType type, string name) =>
      Array.Find(sensors,
          s => s.SensorType == type && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

  // Telemetry throughput sensors report bytes/second; the view shows MB/s (Task Manager parity).
  public static double BytesToMBps(double bytesPerSecond) => bytesPerSecond / BytesPerMB;
}
