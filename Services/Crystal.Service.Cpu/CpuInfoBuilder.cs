using Crystal.Infrastructure.DataStructures.Cpu.Implementations;
using Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cpus;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using Crystal.Provider.CpuId;
using Crystal.Provider.Mmi.HardwareFeatures.Processor;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Smbios.HardwareFeatures.Processor;

namespace Crystal.Service.Cpu;

/// <summary>
/// Assembles the neutral <see cref="ISystemCpuInfo"/> tree - one <see cref="ICpuInfo"/> per populated
/// socket - by pulling from the raw hardware providers and merging their output. It queries CPUID,
/// SMBIOS and WMI for static specs (delegating the reconciliation to <see cref="ICpuSpecsResolver"/>),
/// optionally layers on live sensor readings, and correlates the sources per socket. Every call produces
/// a fresh tree, which is what lets <see cref="CpuMonitor"/> use it both for a one-shot specs build and
/// for repeated sensor polling.
/// </summary>
public sealed class CpuInfoBuilder {
  /// <summary>
  /// Managed CPUID snapshot; authoritative for ISA, family and per-package topology.
  /// </summary>
  private readonly ICpuIdProvider _cpuId;
  /// <summary>
  /// Crystal.Provider.Smbios; decodes SMBIOS Type 4 rows for BIOS-reported speeds and cache.
  /// </summary>
  private readonly ISmbiosProcessorProvider _smbios;
  /// <summary>
  /// Crystal.Provider.Mmi Processor feature; OS-authoritative core/thread counts and firmware flags.
  /// </summary>
  private readonly IWmiHardwareProvider _wmi;
  /// <summary>
  /// Merges the three spec sources above into one neutral <see cref="ICpuSpecs"/> per socket.
  /// </summary>
  private readonly ICpuSpecsResolver _resolver;
  /// <summary>
  /// Optional live sensor source (Crystal.Provider.Telemetry); null builds a specs-only tree.
  /// </summary>
  private readonly ICpuTelemetrySource? _telemetry;

  /// <summary>
  /// Injects the provider snapshots and the specs resolver. <paramref name="telemetry"/> is optional:
  /// when omitted, built trees carry empty sensor holders and only static specs are populated.
  /// </summary>
  /// <param name="cpuId">CPUID snapshot provider.</param>
  /// <param name="smbios">SMBIOS Type 4 processor provider.</param>
  /// <param name="wmi">WMI processor-metrics provider.</param>
  /// <param name="resolver">Reconciles the three spec sources into neutral specs.</param>
  /// <param name="telemetry">Optional live sensor source; null for a specs-only tree.</param>
  public CpuInfoBuilder(ICpuIdProvider cpuId,
                        ISmbiosProcessorProvider smbios,
                        IWmiHardwareProvider wmi,
                        ICpuSpecsResolver resolver,
                        ICpuTelemetrySource? telemetry = null)
    => (_cpuId, _smbios, _wmi, _resolver, _telemetry) = (cpuId, smbios, wmi, resolver, telemetry);

  /// <summary>
  /// Builds a complete CPU tree: queries the providers, refreshes telemetry so sensor values are current,
  /// then emits one <see cref="ICpuInfo"/> per SMBIOS-reported socket. WMI rows are correlated to sockets
  /// by <c>SocketDesignation</c> (not list position, since the two enumerations are not order-aligned),
  /// while live sensors are correlated by ordinal socket index. Missing telemetry falls back to empty
  /// sensor/core holders rather than null.
  /// </summary>
  /// <param name="ct">Token cancelling the asynchronous provider queries.</param>
  /// <returns>A task yielding the freshly built system CPU aggregate.</returns>
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
