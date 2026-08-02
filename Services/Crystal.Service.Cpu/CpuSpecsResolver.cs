using Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cpus;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus;
using Crystal.Provider.CpuId;
using Crystal.Provider.Mmi.HardwareFeatures.Processor;
using Crystal.Provider.Smbios.HardwareFeatures.Processor;

namespace Crystal.Service.Cpu;

public sealed class CpuSpecsResolver : ICpuSpecsResolver {
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

  private static int? ReconcileLogicalCount(uint cpuidCount, uint? wmiCount) {
    // Prefer the OS's own count - it correctly handles processor groups on >64-thread
    // systems, which a single CPUID call (scoped to the executing core's own package) can't see.
    if (wmiCount is > 0) return (int)wmiCount;
    return cpuidCount > 0 ? (int)cpuidCount : null;
  }

  private static int? ReconcilePhysicalCount(uint cpuidCount, uint? wmiCount, int? smbiosCount) {
    if (wmiCount is > 0) return (int)wmiCount;
    if (cpuidCount > 0) return (int)cpuidCount;
    return smbiosCount is > 0 ? smbiosCount : null;
  }
}
