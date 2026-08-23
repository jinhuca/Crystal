using Crystal.Infrastructure.DataStructures.Cpu.Implementations;
using Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cpus;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using Crystal.Provider.CpuId;
using Crystal.Provider.Mmi.HardwareFeatures.Processor;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Smbios.HardwareFeatures.Processor;

namespace Crystal.Service.Cpu;

/// <summary>
/// Orchestrates construction of a complete <see cref="ISystemCpuInfo"/> snapshot by
/// gathering raw CPU data from three independent providers - CPUID, SMBIOS (Type 4),
/// and WMI - reconciling it through an <see cref="ICpuSpecsResolver"/>, and optionally
/// enriching each socket with live sensor telemetry.
/// </summary>
/// <remarks>
/// This class lives in the Service layer by design: <c>DataStructures</c> must stay a
/// zero-dependency leaf, and no individual Provider is aware of the other two, so only
/// the Service layer can see all three providers' raw shapes at once without introducing
/// a dependency cycle.
/// </remarks>
public sealed class CpuInfoBuilder {
  private readonly ICpuIdProvider _cpuId;               // managed CPUID snapshot
  private readonly ISmbiosProcessorProvider _smbios;    // Crystal.Provider.Smbios, Type 4 decoder
  private readonly IWmiHardwareProvider _wmi;           // Crystal.Provider.Mmi, Processor feature
  private readonly ICpuSpecsResolver _resolver;         // Crystal.Service.Cpu, resolves the specs from the three sources above
  private readonly ICpuTelemetrySource? _telemetry;     // optional live sensors (Crystal.Provider.Telemetry)

  /// <summary>
  /// Initializes a new <see cref="CpuInfoBuilder"/> from its three required raw-data
  /// providers, a resolver that reconciles them into final specs, and an optional
  /// live-telemetry source.
  /// </summary>
  /// <param name="cpuId">Provides the managed CPUID snapshot (vendor, family/model/stepping, etc.).</param>
  /// <param name="smbios">Decodes SMBIOS Type 4 (Processor Information) records, one per populated socket.</param>
  /// <param name="wmi">Supplies WMI/MMI Win32_Processor-derived metrics, one per populated socket.</param>
  /// <param name="resolver">Reconciles the CPUID, SMBIOS, and WMI inputs for a given socket into a single authoritative <see cref="ICpuSpecs"/>.</param>
  /// <param name="telemetry">
  /// Optional source of live per-socket/per-core sensor readings (temperatures, clocks,
  /// voltages, etc.). When omitted, sockets are still built but with empty sensor holders.
  /// </param>
  public CpuInfoBuilder(ICpuIdProvider cpuId,
                        ISmbiosProcessorProvider smbios,
                        IWmiHardwareProvider wmi,
                        ICpuSpecsResolver resolver,
                        ICpuTelemetrySource? telemetry = null)
    => (_cpuId, _smbios, _wmi, _resolver, _telemetry) = (cpuId, smbios, wmi, resolver, telemetry);

  /// <summary>
  /// Builds a full <see cref="ISystemCpuInfo"/> snapshot of every populated CPU socket
  /// in the system.
  /// </summary>
  /// <remarks>
  /// Steps performed:
  /// <list type="number">
  ///   <item>Take a single CPUID snapshot (system-wide, not per-socket).</item>
  ///   <item>Fetch all SMBIOS Type 4 processor records - one per populated socket.</item>
  ///   <item>Fetch all WMI processor metrics - one per populated socket.</item>
  ///   <item>Refresh the telemetry source, if one was supplied, so subsequent per-socket reads reflect current values.</item>
  ///   <item>For each SMBIOS socket, correlate the matching WMI record, resolve final specs, attach sensors/cores, and assemble an <see cref="ICpuInfo"/>.</item>
  /// </list>
  /// </remarks>
  /// <param name="ct">Cancellation token propagated to the async provider calls.</param>
  /// <returns>An <see cref="ISystemCpuInfo"/> containing one <see cref="ICpuInfo"/> per populated socket.</returns>
  public async Task<ISystemCpuInfo> BuildAsync(CancellationToken ct) {
    // CPUID is queried once for the whole system (not per-socket) - it reflects
    // whichever logical processor the query happened to run on.
    var cpuidRaw = _cpuId.Query();

    // SMBIOS Type 4 gives one record per *populated* socket, decoded from firmware tables.
    var smbiosProcessors = await _smbios.GetAllProcessorsAsync(ct);          // one row per populated socket

    // WMI/Win32_Processor gives a parallel, independently-ordered view of the same sockets.
    var wmiProcessors = await _wmi.ToProcessorMetricsListAsync(ct);          // one row per populated socket

    // Pull a fresh set of live sensor values (temperatures, clocks, etc.) before reading
    // per-socket/per-core data below, so all sockets in this build reflect the same instant.
    _telemetry?.Refresh();

    var sockets = new List<ICpuInfo>(smbiosProcessors.Count);
    for (int i = 0; i < smbiosProcessors.Count; i++) {
      var smbios = smbiosProcessors[i];

      // Correlate the WMI record to this SMBIOS record by SocketDesignation rather than
      // by list index/position, since WMI and SMBIOS aren't guaranteed to enumerate
      // sockets in the same order.
      var wmi = wmiProcessors.FirstOrDefault(w => w.SocketDesignation == smbios.SocketDesignation);
      // ^ correlate by SocketDesignation, not list position - WMI and SMBIOS aren't
      //   guaranteed to enumerate sockets in the same order.

      // Reconcile the three raw sources (CPUID, SMBIOS, WMI) into one authoritative
      // spec set for this socket.
      var specs = _resolver.Resolve(cpuidRaw, smbios, wmi);

      // Live sensors are correlated by ordinal socket index, matching the
      // Index the Telemetry provider assigns to each processor. When no
      // telemetry source is supplied (or it has no matching processor), fall
      // back to empty sensor holders rather than null.
      var sensors = _telemetry?.GetSensors(i) ?? new CpuSensors();
      var cores = _telemetry?.GetCores(i) ?? [];

      // Assemble the final per-socket info: ordinal index, socket designation from
      // SMBIOS, reconciled specs, and (possibly empty) live sensor/core data.
      sockets.Add(new CpuInfo(i, smbios.SocketDesignation, specs, sensors, cores));
    }

    return new SystemCpuInfo(sockets);
  }
}