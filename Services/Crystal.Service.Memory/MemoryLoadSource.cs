using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Service.Memory;

/// <summary>
/// Reads live physical-memory load from the Telemetry provider (a LibreHardwareMonitor fork).
/// The "Total Memory" hardware exposes a "Memory" <see cref="SensorType.Load"/> sensor (percentage
/// of installed RAM in use) plus "Memory Used" / "Memory Available" <see cref="SensorType.Data"/>
/// sensors reported in GB.
/// </summary>
public sealed class MemoryLoadSource : IMemoryLoadSource, IDisposable {
  /// <summary>
  /// Name of the percentage-in-use <see cref="SensorType.Load"/> sensor.
  /// </summary>
  private const string LoadSensorName = "Memory";

  /// <summary>
  /// Name of the used-GB <see cref="SensorType.Data"/> sensor.
  /// </summary>
  private const string UsedSensorName = "Memory Used";

  /// <summary>
  /// Name of the available-GB <see cref="SensorType.Data"/> sensor.
  /// </summary>
  private const string AvailableSensorName = "Memory Available";

  /// <summary>
  /// The telemetry-provider handle; memory monitoring is enabled and it is opened in the
  /// constructor, so this type owns unmanaged hardware access and must be disposed.
  /// </summary>
  private readonly Computer _computer;

  /// <summary>
  /// Supplies the modified/standby/free page-list figures for the composition bar.
  /// </summary>
  private readonly MemoryCompositionReader _composition = new();

  private bool _disposed;

  /// <summary>
  /// Opens the telemetry provider with memory monitoring enabled. Because this touches
  /// hardware, the type is not test-friendly — <see cref="IMemoryLoadSource"/> exists so callers can
  /// substitute a fake.
  /// </summary>
  public MemoryLoadSource() {
    _computer = new Computer { IsMemoryEnabled = true };
    _computer.Open();
  }

  /// <summary>
  /// Re-samples memory and returns the used percentage plus used/available GB from the
  /// telemetry provider, augmented with the kernel-memory figures (committed, cached, pool,
  /// hardware reserved) from <see cref="KernelMemoryInfo"/>. GB fields are null when unavailable.
  /// </summary>
  public MemoryLoadReading Read() {
    var kernel = KernelMemoryInfo.Read();
    var composition = _composition.Read();

    var memory = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Memory);
    if (memory is null)
      return new MemoryLoadReading(0, null, null,
          kernel.CommittedGB, kernel.CommitLimitGB, kernel.CommitPeakGB, kernel.CachedGB,
          kernel.PagedPoolGB, kernel.NonPagedPoolGB, kernel.HardwareReservedGB,
          kernel.PhysicalTotalGB, composition.ModifiedGB, composition.StandbyGB, composition.FreeGB,
          kernel.PageFileUsedGB, kernel.PageFileTotalGB, kernel.PageFilePeakGB);

    memory.Update();
    var load = FindSensor(memory, SensorType.Load, LoadSensorName);
    var used = FindSensor(memory, SensorType.Data, UsedSensorName);
    var available = FindSensor(memory, SensorType.Data, AvailableSensorName);
    return new MemoryLoadReading(load?.Value ?? 0, used?.Value, available?.Value,
        kernel.CommittedGB, kernel.CommitLimitGB, kernel.CommitPeakGB, kernel.CachedGB,
        kernel.PagedPoolGB, kernel.NonPagedPoolGB, kernel.HardwareReservedGB,
        kernel.PhysicalTotalGB, composition.ModifiedGB, composition.StandbyGB, composition.FreeGB,
        kernel.PageFileUsedGB, kernel.PageFileTotalGB);
  }

  /// <summary>
  /// Finds the first sensor on the memory hardware matching both type and name
  /// (case-insensitive). Returns null when the sensor is absent, letting <see cref="Read"/> fall back
  /// to defaults.
  /// </summary>
  /// <param name="memory">The memory hardware to search.</param>
  /// <param name="type">The sensor type to match.</param>
  /// <param name="name">The sensor name to match, case-insensitively.</param>
  /// <returns>The matching sensor, or null if none is found.</returns>
  private static ISensor? FindSensor(IHardware memory, SensorType type, string name) =>
      Array.Find(memory.Sensors,
          s => s.SensorType == type && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

  /// <summary>
  /// Closes the telemetry provider and disposes the composition reader. Idempotent.
  /// </summary>
  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _composition.Dispose();
    _computer.Close();
  }
}
