using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus;
using Crystal.Provider.CpuId;
using Crystal.Provider.Mmi.HardwareFeatures.Processor;
using Crystal.Provider.Smbios.HardwareFeatures.Processor;

namespace Crystal.Service.Cpu;

public interface ICpuSpecsResolver {
  /// <summary>
  /// Merges CPUID (authoritative for ISA/family), SMBIOS (BIOS-reported speeds
  /// and cache) and WMI (OS-authoritative counts, firmware virtualization flag)
  /// into a single <see cref="ICpuSpecs"/>. SMBIOS/WMI may be null when a socket
  /// has no correlated row.
  /// </summary>
  ICpuSpecs Resolve(CpuIdRawData cpuid, SmbiosProcessorInfo? smbios, WmiProcessorMetrics? wmi);
}
