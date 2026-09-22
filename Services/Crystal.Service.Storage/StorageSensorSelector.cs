using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Service.Storage;

/// <summary>
/// Pure sensor/identifier logic for a storage device's telemetry data, split out from
/// <see cref="StorageLoadSource"/> so it can be unit-tested without opening a hardware
/// <c>Computer</c> or touching performance counters. The source layer keeps only the
/// Update()/enumeration and perf-counter side effects.
/// </summary>
internal static class StorageSensorSelector {
  /// <summary>
  /// Bytes in one megabyte, used to convert throughput sensor values to MB/s.
  /// </summary>
  private const double BytesPerMB = 1024.0 * 1024.0;

  /// <summary>
  /// Extracts the physical-disk number from a telemetry identifier so a live drive can be correlated
  /// to its <c>Win32_DiskDrive.Index</c> inventory entry. Returns null when the trailing token isn't
  /// an integer, signalling the caller to skip that drive.
  /// </summary>
  /// <remarks>
  /// The telemetry Identifier ends with the physical-disk number (StorageDeviceNumber), e.g.
  /// "/nvme/0" -> 0. That's the same value as Win32_DiskDrive.Index, so it joins the two sources.
  /// A trailing token that isn't an integer means the disk can't be correlated → null (skip it).
  /// </remarks>
  /// <param name="identifier">The telemetry hardware identifier for a storage device.</param>
  /// <returns>The physical-disk number, or null when it can't be parsed.</returns>
  public static int? DiskIndexOf(Identifier identifier) {
    var token = identifier.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries)
        .LastOrDefault();
    return int.TryParse(token, out var index) ? index : null;
  }

  /// <summary>
  /// Finds the first sensor of the given type whose name matches (case-insensitively), or
  /// null when the drive doesn't expose it. Matching on both type and name disambiguates sensors that
  /// share a name across categories.
  /// </summary>
  /// <param name="sensors">The drive's sensor array.</param>
  /// <param name="type">The expected sensor type (Load, Throughput, Temperature, …).</param>
  /// <param name="name">The sensor name to match, case-insensitively.</param>
  /// <returns>The matching sensor, or null when none is present.</returns>
  public static ISensor? FindSensor(ISensor[] sensors, SensorType type, string name) =>
      Array.Find(sensors,
          s => s.SensorType == type && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

  /// <summary>
  /// Converts a bytes/second throughput reading to megabytes/second for display parity with Task
  /// Manager.
  /// </summary>
  /// <param name="bytesPerSecond">The raw throughput sensor value in bytes/second.</param>
  /// <returns>The throughput in MB/s.</returns>
  public static double BytesToMBps(double bytesPerSecond) => bytesPerSecond / BytesPerMB;
}
