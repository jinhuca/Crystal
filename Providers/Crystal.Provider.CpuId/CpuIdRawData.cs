using Crystal.Infrastructure.DataStructures.Cpu.Definitions;

namespace Crystal.Provider.CpuId;

/// <summary>
/// Immutable snapshot of the facts the CPUID instruction can report about the
/// executing package. Speed/count fields are 0 when the relevant CPUID leaf is
/// unavailable (e.g. leaf 0x16 is Intel-only), letting callers fall back to
/// SMBIOS/WMI values.
/// </summary>
public sealed record CpuIdRawData(
  string? Brand,
  string? Vendor,
  uint FamilyId,
  uint ModelId,
  uint SteppingId,
  uint BaseSpeedMHz,
  uint BusSpeedMHz,
  uint PhysicalCoreCount,
  uint LogicalCoreCount,
  bool VirtualizationSupported,
  bool VirtualizationFirmwareEnabled,
  CpuCacheInfo? CacheInfo,
  CpuInstructionInfo? InstructionSet);
