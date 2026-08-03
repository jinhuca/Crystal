using Crystal.Provider.Telemetry.Hardware;

namespace GpuModule.Models;

/// <summary>
/// Reads live GPU core-load from the Telemetry provider (a LibreHardwareMonitor fork).
/// Every GPU vendor class exposes a "GPU Core" <see cref="SensorType.Load"/> sensor; we read
/// that per adapter and key it by the adapter's reported name so the view model can pair it
/// with the matching WMI inventory row.
/// <para>Core load does not require the ring-0 driver or elevation, unlike CPU MSR sensors.</para>
/// </summary>
public sealed class GpuLoadSource : IDisposable {
  private const string CoreLoadSensorName = "GPU Core";

  private readonly Computer _computer;
  private bool _disposed;

  public GpuLoadSource() {
    // GPU groups depend on the CPU group being present to detect Intel integrated GPUs
    // (see Computer.IsGpuEnabled), so enable both.
    _computer = new Computer { IsCpuEnabled = true, IsGpuEnabled = true };
    _computer.Open();
  }

  /// <summary>Re-samples every GPU and returns one load reading per adapter.</summary>
  public IReadOnlyList<GpuLoadReading> Read() {
    var readings = new List<GpuLoadReading>();
    foreach (var gpu in EnumerateGpus()) {
      gpu.Update();
      var core = Array.Find(gpu.Sensors,
          s => s.SensorType == SensorType.Load
               && string.Equals(s.Name, CoreLoadSensorName, StringComparison.OrdinalIgnoreCase));
      readings.Add(new GpuLoadReading(gpu.Name, core?.Value ?? 0));
    }
    return readings;
  }

  private IEnumerable<IHardware> EnumerateGpus() =>
      _computer.Hardware.Where(h => h.HardwareType is HardwareType.GpuNvidia
                                                    or HardwareType.GpuAmd
                                                    or HardwareType.GpuIntel);

  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _computer.Close();
  }
}
