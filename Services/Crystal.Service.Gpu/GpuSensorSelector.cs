using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Service.Gpu;

/// <summary>
/// Pure sensor-selection logic for a GPU's telemetry sensor array, split out from
/// <see cref="GpuLoadSource"/> so it can be unit-tested without opening a hardware
/// <c>Computer</c>. Every method takes the adapter's <see cref="ISensor"/> array and returns the
/// value the view model reads; the source layer keeps only the Update()/enumeration side effects.
/// </summary>
internal static class GpuSensorSelector {
  private const string CoreSensorName = "GPU Core";

  // Vendor classes don't all expose overall core utilization the same way: NVIDIA/AMD publish a
  // "GPU Core" Load sensor, but the Intel integrated GPU only publishes per-D3D-engine loads ("3D",
  // "Copy", "Video Decode", …) and no aggregate. Even when "GPU Core" exists it can read 0 while D3D
  // engines are busy (idle Optimus dGPU). So take the max of every Load sensor (excluding memory),
  // mirroring how Task Manager reports GPU utilization; a full VRAM pool must not look like core work.
  public static double SelectCoreLoad(ISensor[] sensors) {
    double max = 0;
    foreach (var s in sensors) {
      if (s.SensorType != SensorType.Load) continue;
      if (s.Name.Contains("Memory", StringComparison.OrdinalIgnoreCase)) continue;
      if (s.Value is { } v && v > max) max = v;
    }
    return max;
  }

  // The core (graphics) clock, in MHz. Every vendor class names it "GPU Core"; other clock sensors
  // ("GPU Memory", "GPU SoC") are ignored. A non-positive reading (0 MHz) is not a real clock — the
  // Intel iGPU's IGCL telemetry can advertise the clock item yet report 0 on some drivers — so treat
  // it as absent and let the view render "—" instead of a misleading 0.00.
  public static double? SelectCoreClock(ISensor[] sensors) {
    var clock = Array.Find(sensors,
        s => s.SensorType == SensorType.Clock
             && string.Equals(s.Name, CoreSensorName, StringComparison.OrdinalIgnoreCase));
    return clock?.Value is { } v && v > 0 ? v : null;
  }

  // Whole-board power draw, in watts. Named "GPU Package" on NVIDIA/AMD and Intel discrete, and
  // "GPU Power" on the Intel integrated class; fall back to "GPU Total" (Intel discrete's board-total
  // rail) so a value shows even when the package rail is absent.
  public static double? SelectPackagePower(ISensor[] sensors) {
    foreach (var name in new[] { "GPU Package", "GPU Power", "GPU Total" }) {
      var power = Array.Find(sensors,
          s => s.SensorType == SensorType.Power
               && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
      if (power?.Value is { } v) return v;
    }
    return null;
  }

  // Prefer the "GPU Core" Temperature sensor (NVIDIA/AMD, and Intel when IGCL telemetry is
  // available). The Intel iGPU only creates that sensor when telemetry succeeds, so fall back to any
  // other temperature sensor the adapter exposes so the graph isn't left blank.
  public static double? SelectCoreTemperature(ISensor[] sensors) {
    var core = Array.Find(sensors,
        s => s.SensorType == SensorType.Temperature
             && string.Equals(s.Name, CoreSensorName, StringComparison.OrdinalIgnoreCase));
    if (core?.Value is { } coreValue) return coreValue;

    foreach (var s in sensors)
      if (s.SensorType == SensorType.Temperature && s.Value is { } v) return v;
    return null;
  }

  // The CPU-package temperature used as the Intel iGPU proxy (the iGPU shares the CPU die and only
  // exposes its own temp under IGCL). Matches the common package aliases HWiNFO reports.
  public static double? SelectCpuPackageTemperature(ISensor[] sensors) {
    var pkg = Array.Find(sensors,
        s => s.SensorType == SensorType.Temperature
             && (string.Equals(s.Name, "CPU Package", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(s.Name, "CPU Cores", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(s.Name, "Core Max", StringComparison.OrdinalIgnoreCase)));
    return pkg?.Value;
  }
}
