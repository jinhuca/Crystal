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
  private readonly Computer _computer;
  private bool _disposed;

  public TelemetryCpuSensorSource() {
    _computer = new Computer { IsCpuEnabled = true };
    _computer.Open();
  }

  public void Refresh() {
    foreach (var cpu in EnumerateCpus())
      cpu.Update();
  }

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

  private IEnumerable<GenericCpu> EnumerateCpus() =>
      _computer.Hardware.OfType<GenericCpu>();

  private GenericCpu? FindCpu(int socketIndex) =>
      EnumerateCpus().FirstOrDefault(c => c.Index == socketIndex);

  private static SensorReading Read(ISensor[] sensors, string hardwareName, SensorType type, params string[] names) {
    ISensor? match = null;
    foreach (var name in names) {
      match = sensors.FirstOrDefault(s => s.SensorType == type
                                          && s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
      if (match is not null) break;
    }
    return CpuTelemetryReadingMapper.ToReading(match, hardwareName, HardwareType.Cpu);
  }

  // The current CPU clock: an AMD package-average clock if the part exposes one,
  // else the fastest individual core clock. "Bus Speed" (the ~100 MHz reference)
  // is excluded because it is not the operating frequency users expect to see.
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

  // The effective (C-state-weighted) core clock: an AMD package-average when
  // present, else the fastest per-core "(Effective)" clock. Distinct from the
  // requested clock in ReadCoreClock; empty on parts that don't expose it.
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

  // The hottest core's thermal headroom: the smallest per-core "Distance to TjMax"
  // reading. Intel-only; empty on parts that don't expose it.
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

  // Per-thread loads for one physical core. SMT threads are named
  // "<core> Thread #<t>" (1-based); a single-threaded core has no suffix, so its
  // lone entry is the core load sensor itself.
  private static IReadOnlyList<SensorReading> ReadThreadLoads(ISensor[] sensors, string hardwareName, string coreName, int threadCount) {
    if (threadCount <= 1)
      return [Read(sensors, hardwareName, SensorType.Load, coreName)];

    var loads = new SensorReading[threadCount];
    for (int t = 0; t < threadCount; t++)
      loads[t] = Read(sensors, hardwareName, SensorType.Load, $"{coreName} Thread #{t + 1}");
    return loads;
  }

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

  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _computer.Close();
  }
}
