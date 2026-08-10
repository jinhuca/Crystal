using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Service.Gpu;

/// <summary>
/// Reads live GPU core-load from the Telemetry provider (a LibreHardwareMonitor fork).
/// Every GPU vendor class exposes a "GPU Core" <see cref="SensorType.Load"/> sensor; we read
/// that per adapter and key it by the adapter's reported name so the view model can pair it
/// with the matching WMI inventory row.
/// <para>Core load does not require the ring-0 driver or elevation, unlike CPU MSR sensors.</para>
/// </summary>
public sealed class GpuLoadSource : IDisposable {
  private const string CoreLoadSensorName = "GPU Core";
  private const string CoreTempSensorName = "GPU Core";

  private readonly Computer _computer;
  private bool _disposed;

  public GpuLoadSource() {
    // GPU groups depend on the CPU group being present to detect Intel integrated GPUs
    // (see Computer.IsGpuEnabled), so enable both.
    _computer = new Computer { IsCpuEnabled = true, IsGpuEnabled = true };
    _computer.Open();
  }

  /// <summary>Re-samples every GPU and returns one load + temperature + clock + power reading
  /// per adapter.</summary>
  public IReadOnlyList<GpuLoadReading> Read() {
    var readings = new List<GpuLoadReading>();
    foreach (var gpu in EnumerateGpus()) {
      gpu.Update();
      var temp = ReadCoreTemperature(gpu)
                 ?? (gpu.HardwareType == HardwareType.GpuIntel ? ReadCpuPackageTemperature() : null);
      readings.Add(new GpuLoadReading(
          gpu.Name, ReadCoreLoad(gpu), temp, ReadCoreClock(gpu), ReadPackagePower(gpu)));
    }
    return readings;
  }

  // An Intel integrated GPU only exposes its own temperature sensor when IGCL telemetry is
  // available; without it, the adapter reports no temperature at all. Since the iGPU shares the
  // CPU die, the CPU package temperature is the standard proxy (what HWiNFO shows for the iGPU).
  // We enable the CPU group anyway (for iGPU detection), so read it here as a fallback.
  private double? ReadCpuPackageTemperature() {
    foreach (var cpu in _computer.Hardware.Where(h => h.HardwareType == HardwareType.Cpu)) {
      cpu.Update();
      var pkg = Array.Find(cpu.Sensors,
          s => s.SensorType == SensorType.Temperature
               && (string.Equals(s.Name, "CPU Package", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(s.Name, "CPU Cores", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(s.Name, "Core Max", StringComparison.OrdinalIgnoreCase)));
      if (pkg?.Value is { } v) return v;
    }
    return null;
  }

  // The vendor classes don't all expose overall core utilization the same way: NVIDIA/AMD publish
  // a "GPU Core" Load sensor, but the Intel integrated GPU only publishes per-D3D-engine loads
  // (named "3D", "Copy", "Video Decode", …) and no aggregate. Even when "GPU Core" exists it can
  // read 0 while D3D engines are busy (idle Optimus dGPU). So take the max of "GPU Core" and the
  // busiest engine load (excluding memory), which mirrors how Windows Task Manager reports GPU
  // utilization. Memory-load sensors are skipped so a full VRAM pool doesn't look like core activity.
  private static double ReadCoreLoad(IHardware gpu) {
    double max = 0;
    foreach (var s in gpu.Sensors) {
      if (s.SensorType != SensorType.Load) continue;
      if (s.Name.Contains("Memory", StringComparison.OrdinalIgnoreCase)) continue;
      if (s.Value is { } v && v > max) max = v;
    }
    return max;
  }

  // The core (graphics) clock, in MHz. Every vendor class names it "GPU Core"; other clock
  // sensors ("GPU Memory", "GPU SoC") are ignored so we report the frequency users associate
  // with GPU speed. A non-positive reading (0 MHz) is not a real clock — the Intel iGPU's IGCL
  // telemetry can advertise the clock item as supported yet report 0 on some drivers — so treat
  // it as absent and let the view render "—" instead of a misleading 0.00.
  private static double? ReadCoreClock(IHardware gpu) {
    var clock = Array.Find(gpu.Sensors,
        s => s.SensorType == SensorType.Clock
             && string.Equals(s.Name, "GPU Core", StringComparison.OrdinalIgnoreCase));
    return clock?.Value is { } v && v > 0 ? v : null;
  }

  // Whole-board power draw, in watts. Named "GPU Package" on NVIDIA/AMD and Intel discrete, and
  // "GPU Power" on the Intel integrated class; fall back to "GPU Total" (Intel discrete's
  // board-total rail) so a value shows even when the package rail is absent.
  private static double? ReadPackagePower(IHardware gpu) {
    foreach (var name in new[] { "GPU Package", "GPU Power", "GPU Total" }) {
      var power = Array.Find(gpu.Sensors,
          s => s.SensorType == SensorType.Power
               && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
      if (power?.Value is { } v) return v;
    }
    return null;
  }

  // Prefer the "GPU Core" Temperature sensor (NVIDIA/AMD, and Intel when IGCL telemetry is
  // available). The Intel iGPU only creates that sensor when telemetry succeeds, so fall back to
  // any other temperature sensor the adapter exposes so the graph isn't left blank.
  private static double? ReadCoreTemperature(IHardware gpu) {
    var core = Array.Find(gpu.Sensors,
        s => s.SensorType == SensorType.Temperature
             && string.Equals(s.Name, CoreTempSensorName, StringComparison.OrdinalIgnoreCase));
    if (core?.Value is { } coreValue) return coreValue;

    foreach (var s in gpu.Sensors)
      if (s.SensorType == SensorType.Temperature && s.Value is { } v) return v;
    return null;
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
