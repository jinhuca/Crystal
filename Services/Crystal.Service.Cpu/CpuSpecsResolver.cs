using Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cpus;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus;
using Crystal.Provider.CpuId;
using Crystal.Provider.Mmi.HardwareFeatures.Processor;
using Crystal.Provider.Smbios.HardwareFeatures.Processor;

namespace Crystal.Service.Cpu;

/// <summary>
/// Reconciles raw CPU data from CPUID, SMBIOS (Type 4), and WMI into a single
/// authoritative <see cref="ICpuSpecs"/> for one socket.
/// </summary>
/// <remarks>
/// Each source has different strengths and blind spots, so this resolver applies a
/// per-field preference order rather than trusting any one source outright:
/// <list type="bullet">
///   <item>CPUID is the ground truth for identity fields (brand/vendor/family/model/stepping) and instruction-set support, but its clock-speed leaf (0x16) is Intel-only/SKU-dependent, and a single CPUID call only sees the package of the executing core - it can't report system-wide topology on multi-processor-group machines.</item>
///   <item>SMBIOS Type 4 is firmware-reported, always present, and is used as the clock-speed fallback and a last-resort core-count fallback.</item>
///   <item>WMI/Win32_Processor reflects the OS's own view and is preferred for core counts and firmware-level virtualization state, since the OS correctly aggregates processor groups on &gt;64-thread systems.</item>
/// </list>
/// </remarks>
public sealed class CpuSpecsResolver : ICpuSpecsResolver {
  /// <summary>
  /// Produces the reconciled <see cref="ICpuSpecs"/> for one socket from its three raw
  /// data sources.
  /// </summary>
  /// <param name="cpuid">Managed CPUID snapshot. Always present - CPUID is queried directly, not via an external provider that could fail to enumerate a socket.</param>
  /// <param name="smbios">SMBIOS Type 4 record for this socket, or <c>null</c> if SMBIOS had no matching entry.</param>
  /// <param name="wmi">WMI Win32_Processor metrics for this socket, or <c>null</c> if WMI had no matching entry.</param>
  /// <returns>A fully populated <see cref="CpuSpecs"/> representing the best available value for each field.</returns>
  public ICpuSpecs Resolve(CpuIdRawData cpuid, SmbiosProcessorInfo? smbios, WmiProcessorMetrics? wmi) {
    return new CpuSpecs {
      // Identity fields come straight from CPUID - it's the authoritative source and
      // doesn't need reconciliation against SMBIOS/WMI for these.
      BrandName = cpuid.Brand,
      VendorName = cpuid.Vendor,
      FamilyId = (int)cpuid.FamilyId,
      ModelId = (int)cpuid.ModelId,
      SteppingId = (int)cpuid.SteppingId,

      // Leaf 0x16 is Intel-only/SKU-dependent; SMBIOS Type 4 always has a BIOS-reported value.
      // Treat a zero/unsupported CPUID reading as "no data" and fall back to SMBIOS.
      BaseSpeed = cpuid.BaseSpeedMHz > 0 ? cpuid.BaseSpeedMHz : smbios?.MaxSpeedMHz,
      BusSpeed = cpuid.BusSpeedMHz > 0 ? cpuid.BusSpeedMHz : smbios?.ExternalClockMHz,

      // Core counts are reconciled via dedicated helpers below - WMI first (OS-aggregated,
      // correct across processor groups), then CPUID, then (physical only) SMBIOS as a
      // last resort.
      PhysicalCoreNum = ReconcilePhysicalCount(cpuid.PhysicalCoreCount, wmi?.NumberOfCores, smbios?.LogicalCoreCount),
      LogicalCoreNum = ReconcileLogicalCount(cpuid.LogicalCoreCount, wmi?.NumberOfLogicalProcessors),

      // "Supported" and "enabled" are different facts - don't collapse them into one bool.
      // Supported: whether the CPU silicon has VT-x/AMD-V at all (CPUID only - WMI has no
      // equivalent concept). Enabled: whether firmware has actually turned it on; prefer
      // WMI's live OS-reported state, falling back to CPUID's own firmware-enablement bit
      // when WMI has no data for this socket.
      VirtualizationSupported = cpuid.VirtualizationSupported,
      VirtualizationEnabled = wmi?.VirtualizationFirmwareEnabled ?? cpuid.VirtualizationFirmwareEnabled,

      // Prefer CPUID's cache leaves (more detailed/reliable) over SMBIOS Type 7 handles.
      CacheInfo = cpuid.CacheInfo ?? smbios?.CacheInfo,

      // Supported instruction-set extensions (SSE/AVX/etc.) are only knowable via CPUID.
      InstructionSet = cpuid.InstructionSet,
    };
  }

  /// <summary>
  /// Resolves the logical (thread) core count, preferring the OS-reported value.
  /// </summary>
  /// <param name="cpuidCount">Logical core count as seen by CPUID from the executing core's own package.</param>
  /// <param name="wmiCount">Logical processor count as reported by WMI/Win32_Processor, or <c>null</c> if unavailable.</param>
  /// <returns>The best available logical core count, or <c>null</c> if neither source reported one.</returns>
  private static int? ReconcileLogicalCount(uint cpuidCount, uint? wmiCount) {
    // Prefer the OS's own count - it correctly handles processor groups on >64-thread
    // systems, which a single CPUID call (scoped to the executing core's own package) can't see.
    if (wmiCount is > 0) return (int)wmiCount;

    // Fall back to CPUID only when WMI has no usable (non-zero) value for this socket.
    return cpuidCount > 0 ? (int)cpuidCount : null;
  }

  /// <summary>
  /// Resolves the physical core count, preferring WMI, then CPUID, then SMBIOS as a last
  /// resort.
  /// </summary>
  /// <param name="cpuidCount">Physical core count as seen by CPUID from the executing core's own package.</param>
  /// <param name="wmiCount">Physical core count as reported by WMI/Win32_Processor, or <c>null</c> if unavailable.</param>
  /// <param name="smbiosCount">
  /// SMBIOS's <c>LogicalCoreCount</c> field, used here as a last-resort fallback for the
  /// *physical* count when neither WMI nor CPUID has a usable value. Note this is
  /// deliberately SMBIOS's logical-count field, not a mismatch to fix without checking
  /// what upstream data is actually available on affected hardware.
  /// </param>
  /// <returns>The best available physical core count, or <c>null</c> if none of the three sources reported one.</returns>
  private static int? ReconcilePhysicalCount(uint cpuidCount, uint? wmiCount, int? smbiosCount) {
    // Same preference order as logical count: OS-reported WMI value wins when present.
    if (wmiCount is > 0) return (int)wmiCount;

    // CPUID is the next-best source, scoped-package caveat aside.
    if (cpuidCount > 0) return (int)cpuidCount;

    // Last resort: SMBIOS. See the smbiosCount parameter doc above regarding the
    // logical-vs-physical field naming here.
    return smbiosCount is > 0 ? smbiosCount : null;
  }
}
