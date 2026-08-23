using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Service.Gpu;

/// <summary>
/// Pure sensor-selection logic for a GPU's telemetry sensor array, split out from
/// <see cref="GpuLoadSource"/> so it can be unit-tested without opening a hardware
/// <c>Computer</c>. Every method takes the adapter's <see cref="ISensor"/> array and returns the
/// value the view model reads; the source layer keeps only the Update()/enumeration side effects.
/// </summary>
internal static class GpuSensorSelector {
  /// <summary>
  /// The core (graphics) load sensor, in percent. Every vendor class exposes a "GPU Core" Load, 
  /// but the Intel integrated GPU only exposes per-D3D-engine loads and no aggregate. 
  /// The view model renders the max of every Load sensor (excluding memory) to mirror Task Manager's 
  /// GPU utilization readout.
  /// </summary>
  private const string CoreSensorName = "GPU Core";

  /// <summary>
  /// Selects the core load from the provided sensors.
  /// Vendor classes don't all expose overall core utilization the same way: NVIDIA/AMD publish a
  /// "GPU Core" Load sensor, but the Intel integrated GPU only publishes per-D3D-engine loads ("3D",
  /// "Copy", "Video Decode", …) and no aggregate. Even when "GPU Core" exists it can read 0 while D3D
  /// engines are busy (idle Optimus dGPU). So take the max of every Load sensor (excluding memory),
  /// mirroring how Task Manager reports GPU utilization; a full VRAM pool must not look like core work.
  /// </summary>
  /// <param name="sensors">The array of sensors.</param>
  /// <returns>The core load percentage.</returns>
  public static double SelectCoreLoad(ISensor[] sensors) {
    double max = 0;
    foreach (var s in sensors) {
      if (s.SensorType != SensorType.Load) {
        continue;
      }
      if (s.Name.Contains("Memory", StringComparison.OrdinalIgnoreCase)) {
        continue;
      }
      if (s.Value is { } v && v > max) {
        max = v;
      }
    }
    return max;
  }

  /// <summary>
  /// Selects the core clock from the provided sensors.
  /// The core (graphics) clock, in MHz. Every vendor class names it "GPU Core"; other clock sensors
  /// ("GPU Memory", "GPU SoC") are ignored. A non-positive reading (0 MHz) is not a real clock — the
  /// Intel iGPU's IGCL telemetry can advertise the clock item yet report 0 on some drivers — so treat
  /// it as absent and let the view render "—" instead of a misleading 0.00.
  /// </summary>
  /// <param name="sensors">The array of sensors.</param>
  /// <returns>The core clock frequency in MHz, or null if not available.</returns>
  public static double? SelectCoreClock(ISensor[] sensors) {
    var clock = Array.Find(
      array: sensors, 
      match: s => s.SensorType == SensorType.Clock && string.Equals(s.Name, CoreSensorName, StringComparison.OrdinalIgnoreCase));
    return clock?.Value is { } v && v > 0 ? v : null;
  }

  /// <summary>
  /// Selects the memory used from the provided sensors.
  /// VRAM used, in GB. Discrete cards expose "GPU Memory Used" (SmallData, MB); the Intel iGPU has no
  /// dedicated VRAM pool, so fall back to the D3D dedicated + shared usage it does report. The
  /// SmallData sensors are in MB, so divide by 1024 for GB.
  /// </summary>
  /// <param name="sensors">The array of sensors.</param>
  /// <returns>The memory used in GB, or null if not available.</returns>
  public static double? SelectMemoryUsedGB(ISensor[] sensors) {
    if (FindSmallData(sensors, "GPU Memory Used") is { } used) {
      return used / 1024.0;
    }

    double? d3d = null;
    if (FindSmallData(sensors, "D3D Dedicated Memory Used") is { } dedicated) d3d = (d3d ?? 0) + dedicated;
    if (FindSmallData(sensors, "D3D Shared Memory Used") is { } shared) d3d = (d3d ?? 0) + shared;
    return d3d is { } v ? v / 1024.0 : null;
  }

  /// <summary>
  /// Selects the total memory from the provided sensors.
  /// VRAM total, in GB. "GPU Memory Total" on discrete cards; the iGPU reports only a shared-memory
  /// limit ("D3D Shared Memory Total"). MB → GB.
  /// </summary>
  /// <param name="sensors">The array of sensors.</param>
  /// <returns>The total memory in GB, or null if not available.</returns>
  public static double? SelectMemoryTotalGB(ISensor[] sensors) {
    var total = FindSmallData(sensors, "GPU Memory Total") ?? FindSmallData(sensors, "D3D Shared Memory Total");
    return total is { } v ? v / 1024.0 : null;
  }

  /// <summary>
  /// Selects the memory (VRAM) clock from the provided sensors.
  /// The memory (VRAM) clock, in MHz. As with the core clock, a non-positive reading is treated as
  /// absent so the view renders "—" rather than a misleading 0.
  /// </summary>
  /// <param name="sensors">The array of sensors.</param>
  /// <returns>The memory clock frequency in MHz, or null if not available.</returns>
  public static double? SelectMemoryClock(ISensor[] sensors) {
    var clock = Array.Find(sensors,
        s => s.SensorType == SensorType.Clock
             && string.Equals(s.Name, "GPU Memory", StringComparison.OrdinalIgnoreCase));
    return clock?.Value is { } v && v > 0 ? v : null;
  }

  /// <summary>
  /// Selects the fan speed from the provided sensors.
  /// Fan speed, in RPM. A card can report several fans (NVIDIA exposes one per cooler); take the
  /// highest so the readout reflects the fan actually spinning. Zero is a valid reading (fan stopped).
  /// </summary>
  /// <param name="sensors">The array of sensors.</param>
  /// <returns>The fan speed in RPM, or null if not available.</returns>
  public static double? SelectFanRpm(ISensor[] sensors) {
    double? max = null;
    foreach (var s in sensors)
      if (s.SensorType == SensorType.Fan && s.Value is { } v && (max is null || v > max)) max = v;
    return max;
  }

  /// <summary>
  /// Selects the core voltage from the provided sensors.
  /// Core voltage, in volts. Named "GPU Core" on AMD/Intel and "GPU Core Voltage" on NVIDIA.
  /// </summary>
  /// <param name="sensors">The array of sensors.</param>
  /// <returns>The core voltage in volts, or null if not available.</returns>
  public static double? SelectCoreVoltage(ISensor[] sensors) {
    var voltage = Array.Find(sensors,
        s => s.SensorType == SensorType.Voltage
             && (string.Equals(s.Name, "GPU Core", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(s.Name, "GPU Core Voltage", StringComparison.OrdinalIgnoreCase)));
    return voltage?.Value;
  }

  /// <summary>
  /// Selects the engine loads from the provided sensors.
  /// Per-engine utilization breakdown. The engines are the D3D queue nodes every adapter exposes
  /// ("D3D 3D", "D3D Video Decode", "D3D Copy", …) plus the Intel discrete class's own
  /// "GPU Render/Compute" and "GPU Media" aggregates. Other Load sensors are deliberately excluded:
  /// "GPU Core" is the aggregate shown separately, "GPU Memory*" is memory not compute, and NVIDIA
  /// publishes power rails ("GPU Power"/"GPU Board Power") as Load-typed sensors that are watts, not
  /// percentages. An adapter can expose several nodes of the same engine type (e.g. two "3D" queues),
  /// so consolidate by display name taking the max — the busiest queue of that type — which also
  /// keeps the value within 0-100. The display name drops the "D3D "/"GPU " prefix. Ordered by name
  /// for a stable readout that doesn't reshuffle every poll.
  /// </summary>  
  /// <param name="sensors">The array of sensors.</param>
  /// <returns>The list of engine loads.</returns>
  public static IReadOnlyList<GpuEngineLoad> SelectEngineLoads(ISensor[] sensors) {
    var byName = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
    foreach (var s in sensors) {
      if (s.SensorType != SensorType.Load || s.Value is not { } v) continue;
      if (!IsEngineLoad(s.Name)) continue;
      var name = EngineDisplayName(s.Name);
      if (!byName.TryGetValue(name, out var existing) || v > existing) byName[name] = v;
    }
    return byName
      .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
      .Select(kv => new GpuEngineLoad(kv.Key, kv.Value))
      .ToArray();
  }

  /// <summary>
  /// Returns true if the sensor name identifies a GPU engine load (D3D queue or Intel discrete
  /// aggregate). Excludes "GPU Core" (the aggregate shown separately), "GPU Memory*" (memory not compute),
  /// and power-related sensors.
  /// </summary>
  /// <param name="name">The sensor name.</param>
  /// <returns>true if the sensor name identifies a GPU engine load; otherwise, false.</returns>
  private static bool IsEngineLoad(string name) =>
    name.StartsWith("D3D ", StringComparison.OrdinalIgnoreCase)
      || string.Equals(name, "GPU Render/Compute", StringComparison.OrdinalIgnoreCase)
      || string.Equals(name, "GPU Media", StringComparison.OrdinalIgnoreCase);

  /// <summary>
  /// Returns the display name for a GPU engine load sensor, dropping the "D3D " or "GPU " prefix.
  /// </summary>
  /// <param name="name">The sensor name.</param>
  /// <returns>The display name.</returns>
  private static string EngineDisplayName(string name) {
    if (name.StartsWith("D3D ", StringComparison.OrdinalIgnoreCase)) return name["D3D ".Length..];
    if (name.StartsWith("GPU ", StringComparison.OrdinalIgnoreCase)) return name["GPU ".Length..];
    return name;
  }

  /// <summary>
  /// Selects the PCIe receive throughput from the provided sensors.
  /// PCIe bus throughput in MB/s. NVIDIA (via NVML) exposes "GPU PCIe Rx"/"GPU PCIe Tx" as
  /// Throughput sensors in bytes/second; other vendors don't publish it. Convert B/s → MB/s (decimal,
  /// matching how throughput is conventionally reported) so the view can render a plain number.
  /// </summary>
  /// <param name="sensors">The array of sensors.</param>
  /// <returns>The PCIe receive throughput in MB/s.</returns>
  public static double? SelectPcieRxMBps(ISensor[] sensors) => SelectThroughputMBps(sensors, "GPU PCIe Rx");

  /// <summary>
  /// Selects the PCIe transmit throughput from the provided sensors.
  /// </summary>
  /// <param name="sensors">The array of sensors.</param>
  /// <returns>The PCIe transmit throughput in MB/s.</returns>
  public static double? SelectPcieTxMBps(ISensor[] sensors) => SelectThroughputMBps(sensors, "GPU PCIe Tx");

  /// <summary>
  /// Selects the throughput sensor with the given name and converts it to MB/s.
  /// </summary>
  /// <param name="sensors">The array of sensors.</param>
  /// <param name="name">The name of the throughput sensor.</param>
  /// <returns>The throughput in MB/s.</returns>
  private static double? SelectThroughputMBps(ISensor[] sensors, string name) {
    var sensor = Array.Find(
      array: sensors,
      match: s => s.SensorType == SensorType.Throughput
             && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
    return sensor?.Value is { } v ? v / 1_000_000.0 : null;
  }

  /// <summary>
  /// Finds a SmallData sensor by name and returns its value.
  /// </summary>
  /// <param name="sensors">The array of sensors.</param>
  /// <param name="name">The name of the SmallData sensor.</param>
  /// <returns>The value of the sensor, or null if not found.</returns>
  private static double? FindSmallData(ISensor[] sensors, string name) {
    var sensor = Array.Find(
      array: sensors,
      match: s => s.SensorType == SensorType.SmallData
             && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
    return sensor?.Value;
  }

  /// <summary>
  /// Selects the package power from the provided sensors.
  /// Whole-board power draw, in watts. Named "GPU Package" on NVIDIA/AMD and Intel discrete, and
  /// "GPU Power" on the Intel integrated class; fall back to "GPU Total" (Intel discrete's board-total
  /// rail) so a value shows even when the package rail is absent.
  /// </summary>
  /// <param name="sensors">The array of sensors.</param>
  /// <returns>The package power, or null if not found.</returns>
  public static double? SelectPackagePower(ISensor[] sensors) {
    foreach (var name in new[] { "GPU Package", "GPU Power", "GPU Total" }) {
      var power = Array.Find(array: sensors, match: s => s.SensorType == SensorType.Power
           && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
      if (power?.Value is { } v) return v;
    }
    return null;
  }

  /// <summary>
  /// The aggregate rails surfaced separately as the single package-power figure; excluded from the
  /// per-rail breakdown so it isn't listed twice.
  /// </summary>
  private static readonly string[] AggregatePowerRails = ["GPU Package", "GPU Power", "GPU Total"];

  /// <summary>
  /// Selects the per-rail power readings from the provided sensors.
  /// Per-rail power breakdown beyond the aggregate package figure: AMD's "GPU Core"/"GPU PPT"/"GPU SoC"
  /// and NVIDIA's "12VHPWR Connector" + per-pin rails. Excludes the aggregate rails shown as the headline 
  /// power number. The "GPU " prefix is dropped for display; ordered by name for a stable readout.
  /// </summary>
  /// <param name="sensors">The array of sensors.</param>
  /// <returns>The list of power rails.</returns>
  public static IReadOnlyList<GpuPowerRail> SelectPowerRails(ISensor[] sensors) {
    var rails = new List<GpuPowerRail>();
    foreach (var s in sensors) {
      if (s.SensorType != SensorType.Power || s.Value is not { } v) continue;
      if (Array.Exists(AggregatePowerRails, n => string.Equals(n, s.Name, StringComparison.OrdinalIgnoreCase))) continue;
      var name = s.Name.StartsWith("GPU ", StringComparison.OrdinalIgnoreCase) ? s.Name["GPU ".Length..] : s.Name;
      rails.Add(new GpuPowerRail(name, v));
    }
    rails.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
    return rails;
  }

  /// <summary>
  /// Selects the core temperature from the provided sensors.
  /// Prefer the "GPU Core" Temperature sensor (NVIDIA/AMD, and Intel when IGCL telemetry is
  /// available). The Intel iGPU only creates that sensor when telemetry succeeds, so fall back to any
  /// other temperature sensor the adapter exposes so the graph isn't left blank.
  /// </summary>
  /// <param name="sensors">The array of sensors.</param>
  /// <returns>The core temperature, or null if not found.</returns>
  public static double? SelectCoreTemperature(ISensor[] sensors) {
    var core = Array.Find(
      array: sensors,
      match: s => s.SensorType == SensorType.Temperature
             && string.Equals(a: s.Name, b: CoreSensorName, comparisonType: StringComparison.OrdinalIgnoreCase));
    if (core?.Value is { } coreValue) return coreValue;

    foreach (var s in sensors)
      if (s.SensorType == SensorType.Temperature && s.Value is { } v) return v;
    return null;
  }

  /// <summary>
  /// Selects the hot-spot temperature from the provided sensors.
  /// Hot-spot (junction) temperature — the hottest on-die sensor, typically running above the core
  /// reading and the first to trigger thermal throttling. Named "GPU Hot Spot" on NVIDIA and AMD.
  /// </summary>
  /// <param name="sensors">The array of sensors.</param>
  /// <returns>The hot-spot temperature, or null if not found.</returns>
  public static double? SelectHotSpotTemperature(ISensor[] sensors) {
    var hotSpot = Array.Find(
      array: sensors,
      match: s => s.SensorType == SensorType.Temperature
             && string.Equals(a: s.Name, b: "GPU Hot Spot", comparisonType: StringComparison.OrdinalIgnoreCase));
    return hotSpot?.Value;
  }

  // VRAM temperature — the memory-junction sensor on cards that expose it. Named "GPU Memory
  // Junction" on NVIDIA and "GPU Memory" on AMD and Intel discrete.
  public static double? SelectMemoryTemperature(ISensor[] sensors) {
    foreach (var name in new[] { "GPU Memory Junction", "GPU Memory" }) {
      var memory = Array.Find(
        array: sensors,
        match: s => s.SensorType == SensorType.Temperature
                     && string.Equals(a: s.Name, b: name, comparisonType: StringComparison.OrdinalIgnoreCase));
      if (memory?.Value is { } v) return v;
    }
    return null;
  }

  // The CPU-package temperature used as the Intel iGPU proxy (the iGPU shares the CPU die and only
  // exposes its own temp under IGCL). Matches the common package aliases HWiNFO reports.
  public static double? SelectCpuPackageTemperature(ISensor[] sensors) {
    var pkg = Array.Find(
      array: sensors,
      match: s => s.SensorType == SensorType.Temperature
             && (string.Equals(a: s.Name, b: "CPU Package", comparisonType: StringComparison.OrdinalIgnoreCase)
                 || string.Equals(a: s.Name, b: "CPU Cores", comparisonType: StringComparison.OrdinalIgnoreCase)
                 || string.Equals(a: s.Name, b: "Core Max", comparisonType: StringComparison.OrdinalIgnoreCase)));
    return pkg?.Value;
  }
}
