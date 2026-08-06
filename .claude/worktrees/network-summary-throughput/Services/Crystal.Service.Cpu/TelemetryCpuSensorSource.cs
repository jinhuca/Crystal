using Crystal.Infrastructure.DataStructures.Cpu.Implementations;
using Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cores;
using Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cpus;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus;
using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Provider.Telemetry.Hardware;
using Crystal.Provider.Telemetry.Hardware.Cpu;

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
      CpuSpeed = Read(sensors, cpu.Name, SensorType.Clock, "Bus Speed"),
      Voltage = Read(sensors, cpu.Name, SensorType.Voltage, "CPU Core", "Core (SVI2 TFN)"),
      PackagePower = Read(sensors, cpu.Name, SensorType.Power, "CPU Package", "Package"),
      CoresPower = Read(sensors, cpu.Name, SensorType.Power, "CPU Cores"),
      MemoryPower = Read(sensors, cpu.Name, SensorType.Power, "CPU Memory"),
      PlatformPower = Read(sensors, cpu.Name, SensorType.Power, "CPU Platform"),
      PackageTemperature = Read(sensors, cpu.Name, SensorType.Temperature, "CPU Package"),
      CoreMaxTemperature = Read(sensors, cpu.Name, SensorType.Temperature, "Core Max", "CCDs Max (Tdie)"),
      CoreAvgTemperature = Read(sensors, cpu.Name, SensorType.Temperature, "Core Average", "CCDs Average (Tdie)"),
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
        Type = lead.CoreType,
        ThreadCount = topology[i].Length,
      };

      var coreSensors = new CoreSensors {
        Name = coreName,
        // SMT cores expose per-thread loads ("... Thread #1/#2"); take the busiest.
        Load = MaxByPrefix(sensors, cpu.Name, SensorType.Load, coreName),
        Speed = Read(sensors, cpu.Name, SensorType.Clock, coreName),
        Temperature = Read(sensors, cpu.Name, SensorType.Temperature, coreName),
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
    return SensorReadingExtensions.ToReading(match, hardwareName, HardwareType.Cpu);
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
    return SensorReadingExtensions.ToReading(best, hardwareName, HardwareType.Cpu);
  }

  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _computer.Close();
  }
}
