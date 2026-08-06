using Crystal.Infrastructure.DataStructures.Cpu.Implementations;
using Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cpus;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using Crystal.Provider.CpuId;
using Crystal.Provider.Mmi.HardwareFeatures.Processor;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Smbios.HardwareFeatures.Processor;

namespace Crystal.Service.Cpu;

public sealed class CpuInfoBuilder {
  private readonly ICpuIdProvider _cpuId;              // managed CPUID snapshot
  private readonly ISmbiosProcessorProvider _smbios;    // Crystal.Provider.Smbios, Type 4 decoder
  private readonly IWmiHardwareProvider _wmi;           // Crystal.Provider.Mmi, Processor feature
  private readonly ICpuSpecsResolver _resolver;
  private readonly ICpuTelemetrySource? _telemetry;     // optional live sensors (Crystal.Provider.Telemetry)

  public CpuInfoBuilder(ICpuIdProvider cpuId, ISmbiosProcessorProvider smbios,
                        IWmiHardwareProvider wmi, ICpuSpecsResolver resolver,
                        ICpuTelemetrySource? telemetry = null)
      => (_cpuId, _smbios, _wmi, _resolver, _telemetry) = (cpuId, smbios, wmi, resolver, telemetry);

  public async Task<ISystemCpuInfo> BuildAsync(CancellationToken ct) {
    var cpuidRaw = _cpuId.Query();
    var smbiosProcessors = await _smbios.GetAllProcessorsAsync(ct);          // one row per populated socket
    var wmiProcessors = await _wmi.ToProcessorMetricsListAsync(ct);          // one row per populated socket

    _telemetry?.Refresh();

    var sockets = new List<ICpuInfo>(smbiosProcessors.Count);
    for (int i = 0; i < smbiosProcessors.Count; i++) {
      var smbios = smbiosProcessors[i];
      var wmi = wmiProcessors.FirstOrDefault(w => w.SocketDesignation == smbios.SocketDesignation);
      // ^ correlate by SocketDesignation, not list position - WMI and SMBIOS aren't
      //   guaranteed to enumerate sockets in the same order.

      var specs = _resolver.Resolve(cpuidRaw, smbios, wmi);

      // Live sensors are correlated by ordinal socket index, matching the
      // Index the Telemetry provider assigns to each processor. When no
      // telemetry source is supplied (or it has no matching processor), fall
      // back to empty sensor holders rather than null.
      var sensors = _telemetry?.GetSensors(i) ?? new CpuSensors();
      var cores = _telemetry?.GetCores(i) ?? [];

      sockets.Add(new CpuInfo(i, smbios.SocketDesignation, specs, sensors, cores));
    }

    return new SystemCpuInfo(sockets);
  }
}
