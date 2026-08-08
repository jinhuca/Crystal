using Crystal.Provider.Telemetry.Hardware;

namespace MemoryModule.Models;

/// <summary>
/// Reads live physical-memory load from the Telemetry provider (a LibreHardwareMonitor fork).
/// The "Total Memory" hardware exposes a "Memory" <see cref="SensorType.Load"/> sensor (percentage
/// of installed RAM in use) plus "Memory Used" / "Memory Available" <see cref="SensorType.Data"/>
/// sensors reported in GB.
/// </summary>
public sealed class MemoryLoadSource : IDisposable {
  private const string LoadSensorName = "Memory";
  private const string UsedSensorName = "Memory Used";
  private const string AvailableSensorName = "Memory Available";

  private readonly Computer _computer;
  private bool _disposed;

  public MemoryLoadSource() {
    _computer = new Computer { IsMemoryEnabled = true };
    _computer.Open();
  }

  /// <summary>Re-samples memory and returns the used percentage plus used/available GB (load 0 and
  /// GB null when unavailable).</summary>
  public MemoryLoadReading Read() {
    var memory = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Memory);
    if (memory is null) return new MemoryLoadReading(0, null, null);

    memory.Update();
    var load = FindSensor(memory, SensorType.Load, LoadSensorName);
    var used = FindSensor(memory, SensorType.Data, UsedSensorName);
    var available = FindSensor(memory, SensorType.Data, AvailableSensorName);
    return new MemoryLoadReading(load?.Value ?? 0, used?.Value, available?.Value);
  }

  private static ISensor? FindSensor(IHardware memory, SensorType type, string name) =>
      Array.Find(memory.Sensors,
          s => s.SensorType == type && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _computer.Close();
  }
}
