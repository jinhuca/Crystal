using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus;
using Crystal.Provider.CpuId;
using Crystal.Provider.Mmi.HardwareFeatures.Processor;
using Crystal.Provider.Smbios.HardwareFeatures.Processor;

namespace Crystal.Service.Cpu;

/// <summary>
/// Reconciles the three independent processor-spec sources - CPUID, SMBIOS and WMI - into one neutral
/// <see cref="ICpuSpecs"/> for a socket. Each source is authoritative for a different subset of facts
/// (CPUID for ISA/family/stepping, SMBIOS for BIOS-reported speeds and cache, WMI for OS-visible core
/// counts and the firmware virtualization flag), and each can disagree or be missing, so a resolver
/// centralizes the merge/precedence rules instead of scattering them across callers.
/// </summary>
public interface ICpuSpecsResolver {
  /// <summary>
  /// Merges CPUID (authoritative for ISA/family), SMBIOS (BIOS-reported speeds
  /// and cache) and WMI (OS-authoritative counts, firmware virtualization flag)
  /// into a single <see cref="ICpuSpecs"/>. SMBIOS/WMI may be null when a socket
  /// has no correlated row.
  /// </summary>
  ICpuSpecs Resolve(CpuIdRawData cpuid, SmbiosProcessorInfo? smbios, WmiProcessorMetrics? wmi);
}
