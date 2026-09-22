using Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cpus;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus;
using Crystal.Provider.CpuId;
using Crystal.Provider.Mmi.HardwareFeatures.Processor;
using Crystal.Provider.Smbios.HardwareFeatures.Processor;

namespace Crystal.Service.Cpu;

/// <summary>
/// Default <see cref="ICpuSpecsResolver"/>. Applies the per-field precedence rules that merge CPUID,
/// SMBIOS and WMI into one <see cref="ICpuSpecs"/>: CPUID wins for ISA/family/stepping, BIOS-reported
/// SMBIOS values backfill speeds and cache when CPUID leaf data is absent, and WMI's OS-visible core
/// counts and firmware virtualization flag override where they are more trustworthy than a single
/// package-scoped CPUID call.
/// </summary>
public sealed class CpuSpecsResolver : ICpuSpecsResolver {
  /// <summary>
  /// Merges the three sources into a neutral <see cref="ICpuSpecs"/>. Speeds prefer the CPUID leaf 0x16
  /// value (Intel-only/SKU-dependent) and fall back to the always-present SMBIOS BIOS-reported speed;
  /// core counts are reconciled by the helpers below; virtualization "supported" (a CPUID capability)
  /// and "enabled" (a firmware setting, WMI-preferred) are kept as distinct facts.
  /// </summary>
  /// <param name="cpuid">Raw CPUID data; authoritative for ISA, family, model and stepping.</param>
  /// <param name="smbios">SMBIOS Type 4 row, or null when no correlated socket exists.</param>
  /// <param name="wmi">WMI processor metrics, or null when no correlated socket exists.</param>
  /// <returns>The merged, neutral CPU specs.</returns>
  public ICpuSpecs Resolve(CpuIdRawData cpuid, SmbiosProcessorInfo? smbios, WmiProcessorMetrics? wmi) {
    return new CpuSpecs {
      BrandName = cpuid.Brand,
      VendorName = cpuid.Vendor,
      FamilyId = (int)cpuid.FamilyId,
      ModelId = (int)cpuid.ModelId,
      SteppingId = (int)cpuid.SteppingId,

      // Leaf 0x16 is Intel-only/SKU-dependent; SMBIOS Type 4 always has a BIOS-reported value.
      BaseSpeed = cpuid.BaseSpeedMHz > 0 ? cpuid.BaseSpeedMHz : smbios?.MaxSpeedMHz,
      BusSpeed = cpuid.BusSpeedMHz > 0 ? cpuid.BusSpeedMHz : smbios?.ExternalClockMHz,

      PhysicalCoreNum = ReconcilePhysicalCount(cpuid.PhysicalCoreCount, wmi?.NumberOfCores, smbios?.LogicalCoreCount),
      LogicalCoreNum = ReconcileLogicalCount(cpuid.LogicalCoreCount, wmi?.NumberOfLogicalProcessors),

      // "Supported" and "enabled" are different facts - don't collapse them into one bool.
      VirtualizationSupported = cpuid.VirtualizationSupported,
      VirtualizationEnabled = wmi?.VirtualizationFirmwareEnabled ?? cpuid.VirtualizationFirmwareEnabled,

      CacheInfo = cpuid.CacheInfo ?? smbios?.CacheInfo,
      InstructionSet = cpuid.InstructionSet,
    };
  }

  /// <summary>
  /// Picks the logical-processor (thread) count, preferring the OS's own figure over CPUID.
  /// </summary>
  /// <param name="cpuidCount">The CPUID-reported logical count (package-scoped).</param>
  /// <param name="wmiCount">The OS-reported logical count, if available.</param>
  /// <returns>The chosen count, or <see langword="null"/> when neither source has a positive value.</returns>
  private static int? ReconcileLogicalCount(uint cpuidCount, uint? wmiCount) {
    // Prefer the OS's own count - it correctly handles processor groups on >64-thread
    // systems, which a single CPUID call (scoped to the executing core's own package) can't see.
    if (wmiCount is > 0) return (int)wmiCount;
    return cpuidCount > 0 ? (int)cpuidCount : null;
  }

  /// <summary>
  /// Picks the physical-core count in priority order WMI, then CPUID, then SMBIOS - each used only when
  /// it reports a positive value - so the most trustworthy available source wins.
  /// </summary>
  /// <param name="cpuidCount">The CPUID-reported physical core count.</param>
  /// <param name="wmiCount">The OS-reported physical core count, if available.</param>
  /// <param name="smbiosCount">The SMBIOS-reported count used as a last resort.</param>
  /// <returns>The chosen count, or <see langword="null"/> when no source has a positive value.</returns>
  private static int? ReconcilePhysicalCount(uint cpuidCount, uint? wmiCount, int? smbiosCount) {
    if (wmiCount is > 0) return (int)wmiCount;
    if (cpuidCount > 0) return (int)cpuidCount;
    return smbiosCount is > 0 ? smbiosCount : null;
  }
}
