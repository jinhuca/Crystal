using System.Collections.Frozen;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus;
using Crystal.Provider.CpuId;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Smbios.HardwareFeatures.Processor;
using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Service.Cpu.Tests;

// Minimal ISensor stand-in for exercising CpuTelemetryReadingMapper. Only Name/SensorType/Value/
// Min/Max are meaningful; the rest satisfy the interface. Mirrors the other test projects' StubSensor.
internal sealed class StubSensor : ISensor {
  public string Name { get; set; } = string.Empty;
  public SensorType SensorType { get; set; }
  public float? Value { get; set; }
  public float? Min { get; set; }
  public float? Max { get; set; }

  public IControl Control => null!;
  public IHardware Hardware => null!;
  public Identifier Identifier => new("stub", "sensor");
  public int Index => 0;
  public bool IsDefaultHidden => false;
  public IReadOnlyList<IParameter> Parameters => Array.Empty<IParameter>();
  public IEnumerable<SensorValue> Values => Array.Empty<SensorValue>();
  public TimeSpan ValuesTimeWindow { get; set; }

  public void ResetMin() { }
  public void ResetMax() { }
  public void ClearValues() { }
  public void Accept(IVisitor visitor) { }
  public void Traverse(IVisitor visitor) { }
}

internal sealed class FakeCpuIdProvider(CpuIdRawData data) : ICpuIdProvider {
  public int QueryCount { get; private set; }
  public CpuIdRawData Query() {
    QueryCount++;
    return data;
  }
}

internal sealed class FakeSmbiosProcessorProvider(IReadOnlyList<SmbiosProcessorInfo> processors)
    : ISmbiosProcessorProvider {
  public Task<IReadOnlyList<SmbiosProcessorInfo>> GetAllProcessorsAsync(CancellationToken cancellationToken)
    => Task.FromResult(processors);
}

// CpuInfoBuilder calls the ToProcessorMetricsListAsync extension, which under the hood invokes
// GetMultiMetricsForClassAsync(Win32_Processor). So the fake speaks the raw WmiValue property-bag
// contract rather than returning WmiProcessorMetrics directly. Keys match Win32_Processor property
// names (WmiProcessor.* consts, which are internal nameof values).
internal sealed class FakeWmiHardwareProvider(IReadOnlyList<FrozenDictionary<string, WmiValue>> instances)
    : IWmiHardwareProvider {
  public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
      string wmiClassName, CancellationToken cancellationToken, bool bypassCache = false,
      IReadOnlyList<string>? projection = null)
    => Task.FromResult(instances);

  public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
      string namespaceName, string wmiClassName, CancellationToken cancellationToken)
    => Task.FromResult(instances);

  public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> QueryAsync(
      string namespaceName, string wqlQuery, CancellationToken cancellationToken)
    => throw new NotSupportedException();

  public Task<WmiMethodResult> InvokeStaticMethodAsync(
      string namespaceName, string wmiClassName, string methodName,
      IReadOnlyDictionary<string, WmiValue> inParameters, CancellationToken cancellationToken)
    => throw new NotSupportedException();

  public static FrozenDictionary<string, WmiValue> ProcessorRow(
      string socketDesignation, int logicalProcessors, int cores) =>
    new Dictionary<string, WmiValue> {
      ["SocketDesignation"] = new(socketDesignation),
      ["NumberOfLogicalProcessors"] = new(logicalProcessors),
      ["NumberOfCores"] = new(cores),
    }.ToFrozenDictionary();
}

// Records which socket indices were queried and whether Refresh ran, so tests can assert the
// builder's per-socket correlation and its refresh-before-read ordering.
internal sealed class FakeCpuTelemetrySource : ICpuTelemetrySource {
  private readonly Dictionary<int, ICpuSensors> _sensors;
  private readonly Dictionary<int, IReadOnlyList<ICoreInfo>> _cores;

  public FakeCpuTelemetrySource(
      Dictionary<int, ICpuSensors>? sensors = null,
      Dictionary<int, IReadOnlyList<ICoreInfo>>? cores = null) {
    _sensors = sensors ?? new();
    _cores = cores ?? new();
  }

  public bool Refreshed { get; private set; }
  public bool Disposed { get; private set; }
  public List<int> RequestedSensorIndices { get; } = new();

  public void Refresh() => Refreshed = true;

  public ICpuSensors? GetSensors(int socketIndex) {
    RequestedSensorIndices.Add(socketIndex);
    return _sensors.TryGetValue(socketIndex, out var s) ? s : null;
  }

  public IReadOnlyList<ICoreInfo> GetCores(int socketIndex)
    => _cores.TryGetValue(socketIndex, out var c) ? c : [];

  public void Dispose() => Disposed = true;
}
