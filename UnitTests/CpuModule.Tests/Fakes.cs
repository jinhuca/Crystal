using System.Collections.Frozen;
using Crystal.Infrastructure.DataStructures.Cpu.Definitions;
using Crystal.Infrastructure.DataStructures.Cpu.Implementations;
using Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cores;
using Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cpus;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Provider.CpuId;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Smbios.HardwareFeatures.Processor;

namespace CpuModule.Tests;

// Builds ISystemCpuInfo trees from the concrete Infrastructure implementations (all plain
// constructible types), so the CpuModule view models can be exercised against realistic input
// without touching a provider.
internal static class Fakes {
  private static SensorReading Reading(float? value) =>
      new(string.Empty, HardwareType.Cpu, string.Empty, SensorType.Load, value, null, null, null);

  // A single-socket system. Any argument left null leaves the underlying sensor reading empty
  // (Value == null), mirroring an MSR-less machine.
  public static SystemCpuInfo System(
      CpuSpecs? specs = null,
      float? load = null, float? voltage = null, float? speedMHz = null,
      float? power = null, float? temperature = null,
      IReadOnlyList<float?>? coreLoads = null,
      int socketIndex = 0) {
    var sensors = new CpuSensors {
      TotalLoad = Reading(load),
      Voltage = Reading(voltage),
      CpuSpeed = Reading(speedMHz),
      PackagePower = Reading(power),
      PackageTemperature = Reading(temperature),
    };

    var cores = (coreLoads ?? [])
        .Select(l => (ICoreInfo)new CoreInfo(new CoreSpecs(), new CoreSensors { Load = Reading(l) }))
        .ToList();

    var socket = new CpuInfo(socketIndex, $"CPU{socketIndex}", specs ?? new CpuSpecs(), sensors, cores);
    return new SystemCpuInfo([socket]);
  }

  // A system with no sockets at all — the view models must no-op on this.
  public static SystemCpuInfo Empty() => new([]);

  public static CpuCacheInfo Cache(int l1Bytes, int l2Bytes, int l3Bytes, int l1LineSize) =>
      new() {
        L1_cache_size = l1Bytes,
        L2_cache_size = l2Bytes,
        L3_cache_size = l3Bytes,
        L1_cache_line_size = l1LineSize,
      };
}

// Provider-level fakes for the end-to-end pipeline tests, which build a real CpuInfoBuilder rather
// than a pre-made ISystemCpuInfo tree. Mirror the fakes in Crystal.Service.Cpu.Tests (which are
// internal to that assembly and so not visible here).
internal sealed class FakeCpuIdProvider(CpuIdRawData data) : ICpuIdProvider {
  public CpuIdRawData Query() => data;
}

internal sealed class FakeSmbiosProcessorProvider(IReadOnlyList<SmbiosProcessorInfo> processors)
    : ISmbiosProcessorProvider {
  public Task<IReadOnlyList<SmbiosProcessorInfo>> GetAllProcessorsAsync(CancellationToken cancellationToken)
    => Task.FromResult(processors);
}

// CpuInfoBuilder correlates WMI rows via the raw WmiValue property-bag contract
// (GetMultiMetricsForClassAsync(Win32_Processor)), so the fake speaks that rather than returning
// typed metrics. Keys match Win32_Processor property names.
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
