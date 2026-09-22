using Crystal.Infrastructure.DataStructures.Cpu.Implementations;
using Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cores;
using Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cpus;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus;
using Crystal.Provider.Telemetry.Hardware;
using Crystal.Provider.Telemetry.Hardware.Cpu;
using SensorReading = Crystal.Infrastructure.DataStructures.Sensors.SensorReading;

namespace Crystal.Service.Cpu;

/// <summary>
/// Reads live CPU/core sensors from the Telemetry provider (a
/// LibreHardwareMonitor fork) and projects them onto the neutral
/// <see cref="Crystal.Infrastructure.DataStructures.Sensors.SensorReading"/> types.
/// <para>
/// Sockets are matched to Telemetry processors by <see cref="GenericCpu.Index"/>,
/// which is the same ordinal <see cref="CpuInfoBuilder"/> assigns. Temperature,
/// clock, voltage and power come from MSRs via the ring-0 driver, so they read
/// as empty unless the process is elevated; per-core load does not need elevation.
/// </para>
/// </summary>
public sealed class TelemetryCpuSensorSource : ICpuTelemetrySource {
  /// <summary>
  /// The open LibreHardwareMonitor session with only CPU hardware enabled. Owns the underlying ring-0
  /// driver handle, so it must be closed on <see cref="Dispose"/>.
  /// </summary>
  private readonly Computer _computer;

  /// <summary>
  /// Guards against closing the session twice on repeated <see cref="Dispose"/> calls.
  /// </summary>
  private bool _disposed;

  /// <summary>
  /// Opens the hardware session with CPU monitoring enabled. Opening loads the ring-0 driver; MSR-backed
  /// sensors (temperature, clock, voltage, power) only produce values when the process is elevated.
  /// </summary>
  public TelemetryCpuSensorSource() {
    _computer = new Computer { IsCpuEnabled = true };
    _computer.Open();
  }

  /// <summary>
  /// Re-samples every CPU by calling <c>Update()</c> on each provider hardware node. Must be called
  /// before <see cref="GetSensors"/>/<see cref="GetCores"/> to observe fresh values.
  /// </summary>
  public void Refresh() {
    foreach (var cpu in EnumerateCpus())
      cpu.Update();
  }

  /// <summary>
  /// Reads package-level sensors for the socket at <paramref name="socketIndex"/> and maps them onto a
  /// neutral <see cref="CpuSensors"/>. Sensor names differ between Intel and AMD, so each field probes a
  /// vendor-specific list of candidate names; unmatched sensors yield empty readings. Returns
  /// <see langword="null"/> when no provider CPU has that ordinal index.
  /// </summary>
  /// <param name="socketIndex">Ordinal socket index matching the provider's <see cref="GenericCpu.Index"/>.</param>
  /// <returns>The package sensor snapshot, or <see langword="null"/> when the socket is not present.</returns>
  public ICpuSensors? GetSensors(int socketIndex) {
    var cpu = FindCpu(socketIndex);
    if (cpu is null) return null;

    var sensors = cpu.Sensors;
    return new CpuSensors {
      // "Bus Speed" is only the ~100 MHz reference clock; the meaningful CPU speed
      // is the current core clock. Prefer an AMD package-average clock when present,
      // otherwise take the fastest per-core clock (the boosting core).
      CpuSpeed = ReadCoreClock(sensors, cpu.Name),
      CpuEffectiveSpeed = ReadEffectiveClock(sensors, cpu.Name),
      // BCLK: the reference clock ReadCoreClock deliberately skips as the operating frequency.
      BusSpeed = Read(sensors, cpu.Name, SensorType.Clock, "Bus Speed"),
      Voltage = Read(sensors, cpu.Name, SensorType.Voltage, "CPU Core", "Core (SVI2 TFN)"),
      SocVoltage = Read(sensors, cpu.Name, SensorType.Voltage, "SoC (SVI2 TFN)"),
      PackagePower = Read(sensors, cpu.Name, SensorType.Power, "CPU Package", "Package"),
      CoresPower = Read(sensors, cpu.Name, SensorType.Power, "CPU Cores"),
      GraphicsPower = Read(sensors, cpu.Name, SensorType.Power, "CPU Graphics"),
      MemoryPower = Read(sensors, cpu.Name, SensorType.Power, "CPU Memory"),
      PlatformPower = Read(sensors, cpu.Name, SensorType.Power, "CPU Platform"),
      PackageTemperature = Read(sensors, cpu.Name, SensorType.Temperature, "CPU Package"),
      CoreMaxTemperature = Read(sensors, cpu.Name, SensorType.Temperature, "Core Max", "CCDs Max (Tdie)"),
      CoreAvgTemperature = Read(sensors, cpu.Name, SensorType.Temperature, "Core Average", "CCDs Average (Tdie)"),
      MinDistanceToTjMax = ReadMinDistanceToTjMax(sensors, cpu.Name),
      // Package throttle-reason flags (Intel): 0/1 Factor sensors, empty when unavailable.
      ThermalThrottling = Read(sensors, cpu.Name, SensorType.Factor, "Thermal Throttling"),
      PowerLimitThrottling = Read(sensors, cpu.Name, SensorType.Factor, "Power Limit Throttling"),
      Prochot = Read(sensors, cpu.Name, SensorType.Factor, "PROCHOT"),
      // Configured RAPL power limits (Intel): watts, empty when unavailable.
      PowerLimitLong = Read(sensors, cpu.Name, SensorType.Power, "Power Limit (Long)"),
      PowerLimitShort = Read(sensors, cpu.Name, SensorType.Power, "Power Limit (Short)"),
      // Package current (AMD SMU): TDC/EDC in A, empty on parts that don't expose them.
      Tdc = Read(sensors, cpu.Name, SensorType.Current, "TDC"),
      Edc = Read(sensors, cpu.Name, SensorType.Current, "EDC"),
      // Package C-state residency (%): Intel MSR counters; empty on parts/states not exposed.
      PackageC2Residency = Read(sensors, cpu.Name, SensorType.Level, "CPU Package C2"),
      PackageC3Residency = Read(sensors, cpu.Name, SensorType.Level, "CPU Package C3"),
      PackageC6Residency = Read(sensors, cpu.Name, SensorType.Level, "CPU Package C6"),
      PackageC7Residency = Read(sensors, cpu.Name, SensorType.Level, "CPU Package C7"),
      TotalLoad = Read(sensors, cpu.Name, SensorType.Load, "CPU Total"),
      CoreMaxLoad = Read(sensors, cpu.Name, SensorType.Load, "CPU Core Max"),
    };
  }

  /// <summary>
  /// Builds one <see cref="ICoreInfo"/> per physical core for the socket at <paramref name="socketIndex"/>,
  /// pairing topology (APIC id, hybrid core type, thread count) with per-core sensor readings. The
  /// provider's per-core sensor naming is quirky and vendor-dependent - generic "CPU Core #n" for
  /// clock/load/temperature, AMD "Core #n" for effective clock/multiplier/power - so each field probes
  /// the appropriate candidate names. Returns an empty list when the socket is not present.
  /// </summary>
  /// <param name="socketIndex">Ordinal socket index matching the provider's <see cref="GenericCpu.Index"/>.</param>
  /// <returns>Per-core info rows, or an empty list when the socket is not present.</returns>
  public IReadOnlyList<ICoreInfo> GetCores(int socketIndex) {
    var cpu = FindCpu(socketIndex);
    if (cpu is null) return [];

    var sensors = cpu.Sensors;
    var topology = cpu.CpuId; // outer index = physical core, inner = threads on that core
    var cores = new List<ICoreInfo>(topology.Length);

    for (int i = 0; i < topology.Length; i++) {
      var lead = topology[i][0];
      // The base GenericCpu labels per-core sensors "CPU Core #<n>" (or just
      // "CPU Core" on single-core parts); vendor classes reuse that name for
      // clock/temperature/voltage, so we match on it.
      string coreName = topology.Length == 1 ? "CPU Core" : $"CPU Core #{i + 1}";

      var specs = new CoreSpecs {
        CoreIndex = i,
        ApicId = (int)lead.ApicId,
        Type = CpuTelemetryReadingMapper.ToAppCoreType(lead.CoreType),
        ThreadCount = topology[i].Length,
      };

      var coreSensors = new CoreSensors {
        Name = coreName,
        // SMT cores expose per-thread loads ("... Thread #1/#2"); take the busiest.
        Load = MaxByPrefix(sensors, cpu.Name, SensorType.Load, coreName),
        ThreadLoads = ReadThreadLoads(sensors, cpu.Name, coreName, topology[i].Length),
        Speed = Read(sensors, cpu.Name, SensorType.Clock, coreName),
        // Effective clock and multiplier are AMD-only and use the vendor's
        // "Core #<n>" naming (n is 0-based), unlike the generic "CPU Core #n".
        EffectiveSpeed = Read(sensors, cpu.Name, SensorType.Clock,
            $"{coreName} (Effective)", $"Core #{i} (Effective)", $"Core #{i + 1} (Effective)"),
        Multiplier = Read(sensors, cpu.Name, SensorType.Factor, coreName, $"Core #{i}", $"Core #{i + 1}"),
        Temperature = Read(sensors, cpu.Name, SensorType.Temperature, coreName),
        // Intel exposes per-core thermal headroom as "<core> Distance to TjMax".
        DistanceToTjMax = Read(sensors, cpu.Name, SensorType.Temperature, $"{coreName} Distance to TjMax"),
        // Per-core power is AMD-only, from the SMU, named "Core #<n> (SMU)" (n is 0-based).
        Power = Read(sensors, cpu.Name, SensorType.Power, $"Core #{i} (SMU)", $"Core #{i + 1} (SMU)"),
        Voltage = Read(sensors, cpu.Name, SensorType.Voltage, coreName),
      };

      cores.Add(new CoreInfo(specs, coreSensors));
    }

    return cores;
  }

  /// <summary>
  /// Yields the CPU hardware nodes from the provider session, filtering out non-CPU hardware.
  /// </summary>
  private IEnumerable<GenericCpu> EnumerateCpus() =>
      _computer.Hardware.OfType<GenericCpu>();

  /// <summary>
  /// Locates the provider CPU whose ordinal <see cref="GenericCpu.Index"/> matches, or null.
  /// </summary>
  private GenericCpu? FindCpu(int socketIndex) =>
      EnumerateCpus().FirstOrDefault(c => c.Index == socketIndex);

  /// <summary>
  /// Finds the first sensor of <paramref name="type"/> whose name matches (case-insensitively) any of the
  /// candidate <paramref name="names"/>, in order, and maps it to a neutral reading. The candidate list
  /// absorbs Intel/AMD naming differences; an empty reading is returned when none match.
  /// </summary>
  /// <param name="sensors">The socket's sensor array to search.</param>
  /// <param name="hardwareName">Owning hardware name stamped onto the reading.</param>
  /// <param name="type">The sensor type to match.</param>
  /// <param name="names">Candidate sensor names tried in priority order.</param>
  /// <returns>The matched reading, or an empty reading when none matched.</returns>
  private static SensorReading Read(ISensor[] sensors, string hardwareName, SensorType type, params string[] names) {
    ISensor? match = null;
    foreach (var name in names) {
      match = sensors.FirstOrDefault(s => s.SensorType == type
                                          && s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
      if (match is not null) break;
    }
    return CpuTelemetryReadingMapper.ToReading(match, hardwareName, HardwareType.Cpu);
  }

  /// <summary>
  /// The current CPU clock: an AMD package-average clock if the part exposes one, else the fastest
  /// individual core clock. "Bus Speed" (the ~100 MHz reference) is excluded because it is not the
  /// operating frequency users expect to see.
  /// </summary>
  private static SensorReading ReadCoreClock(ISensor[] sensors, string hardwareName) {
    var avg = sensors.FirstOrDefault(s => s.SensorType == SensorType.Clock
                                          && s.Name.Equals("Cores (Average)", StringComparison.OrdinalIgnoreCase));
    if (avg is not null)
      return CpuTelemetryReadingMapper.ToReading(avg, hardwareName, HardwareType.Cpu);

    ISensor? best = null;
    foreach (var s in sensors) {
      if (s.SensorType != SensorType.Clock) continue;
      if (s.Name.StartsWith("Bus", StringComparison.OrdinalIgnoreCase)) continue;
      if (best is null || (s.Value ?? 0) > (best.Value ?? 0)) best = s;
    }
    return CpuTelemetryReadingMapper.ToReading(best, hardwareName, HardwareType.Cpu);
  }

  /// <summary>
  /// The effective (C-state-weighted) core clock: an AMD package-average when present, else the fastest
  /// per-core "(Effective)" clock. Distinct from the requested clock in <see cref="ReadCoreClock"/>;
  /// empty on parts that don't expose it.
  /// </summary>
  private static SensorReading ReadEffectiveClock(ISensor[] sensors, string hardwareName) {
    var avg = sensors.FirstOrDefault(s => s.SensorType == SensorType.Clock
                                          && s.Name.Equals("Cores (Average Effective)", StringComparison.OrdinalIgnoreCase));
    if (avg is not null)
      return CpuTelemetryReadingMapper.ToReading(avg, hardwareName, HardwareType.Cpu);

    ISensor? best = null;
    foreach (var s in sensors) {
      if (s.SensorType != SensorType.Clock) continue;
      if (!s.Name.EndsWith("(Effective)", StringComparison.OrdinalIgnoreCase)) continue;
      if (best is null || (s.Value ?? 0) > (best.Value ?? 0)) best = s;
    }
    return CpuTelemetryReadingMapper.ToReading(best, hardwareName, HardwareType.Cpu);
  }

  /// <summary>
  /// The hottest core's thermal headroom: the smallest per-core "Distance to TjMax" reading. Intel-only;
  /// empty on parts that don't expose it.
  /// </summary>
  private static SensorReading ReadMinDistanceToTjMax(ISensor[] sensors, string hardwareName) {
    ISensor? best = null;
    foreach (var s in sensors) {
      if (s.SensorType != SensorType.Temperature) continue;
      if (!s.Name.EndsWith("Distance to TjMax", StringComparison.OrdinalIgnoreCase)) continue;
      if (s.Value is null) continue;
      if (best is null || s.Value < best.Value) best = s;
    }
    return CpuTelemetryReadingMapper.ToReading(best, hardwareName, HardwareType.Cpu);
  }

  /// <summary>
  /// Per-thread loads for one physical core. SMT threads are named "&lt;core&gt; Thread #&lt;t&gt;"
  /// (1-based); a single-threaded core has no suffix, so its lone entry is the core load sensor itself.
  /// </summary>
  private static IReadOnlyList<SensorReading> ReadThreadLoads(ISensor[] sensors, string hardwareName, string coreName, int threadCount) {
    if (threadCount <= 1)
      return [Read(sensors, hardwareName, SensorType.Load, coreName)];

    var loads = new SensorReading[threadCount];
    for (int t = 0; t < threadCount; t++)
      loads[t] = Read(sensors, hardwareName, SensorType.Load, $"{coreName} Thread #{t + 1}");
    return loads;
  }

  /// <summary>
  /// The highest-valued sensor of <paramref name="type"/> whose name starts with
  /// <paramref name="namePrefix"/>. Used to fold SMT per-thread rows into one core load by taking the
  /// busiest thread. Guards against a prefix like "CPU Core #1" spuriously matching "CPU Core #10" by
  /// rejecting a following digit.
  /// </summary>
  private static SensorReading MaxByPrefix(ISensor[] sensors, string hardwareName, SensorType type, string namePrefix) {
    ISensor? best = null;
    foreach (var s in sensors) {
      if (s.SensorType != type) continue;
      if (!s.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase)) continue;
      // Guard against "CPU Core #1" also matching "CPU Core #10+": require the
      // next char (if any) to be a separator, not another digit.
      if (s.Name.Length > namePrefix.Length && char.IsDigit(s.Name[namePrefix.Length])) continue;
      if (best is null || (s.Value ?? 0) > (best.Value ?? 0)) best = s;
    }
    return CpuTelemetryReadingMapper.ToReading(best, hardwareName, HardwareType.Cpu);
  }

  /// <summary>
  /// Closes the LibreHardwareMonitor session, releasing the ring-0 driver handle. Idempotent - guarded by
  /// <see cref="_disposed"/> so repeated calls are safe.
  /// </summary>
  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _computer.Close();
  }
}
