using Crystal.Provider.Telemetry.Hardware;

namespace MemoryModule.Models;

/// <summary>
/// Reads live physical-memory load from the Telemetry provider (a LibreHardwareMonitor fork).
/// The "Total Memory" hardware exposes a "Memory" <see cref="SensorType.Load"/> sensor whose
/// value is the percentage of installed RAM currently in use.
/// </summary>
public sealed class MemoryLoadSource : IDisposable {
  private const string LoadSensorName = "Memory";

  private readonly Computer _computer;
  private bool _disposed;

  public MemoryLoadSource() {
    _computer = new Computer { IsMemoryEnabled = true };
    _computer.Open();
  }

  /// <summary>Re-samples memory and returns the used-percentage (0 if unavailable).</summary>
  public double Read() {
    var memory = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Memory);
    if (memory is null) return 0;

    memory.Update();
    var load = Array.Find(memory.Sensors,
        s => s.SensorType == SensorType.Load
             && string.Equals(s.Name, LoadSensorName, StringComparison.OrdinalIgnoreCase));
    return load?.Value ?? 0;
  }

  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _computer.Close();
  }
}
