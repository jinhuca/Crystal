using Crystal.Infrastructure.DataStructures.Cpu.Definitions;

namespace Crystal.Provider.Smbios.HardwareFeatures.Processor;

/// <summary>
/// One populated processor socket as reported by SMBIOS Type 4, with the
/// associated Type 7 cache sizes already resolved via the L1/L2/L3 handles.
/// Speeds are the BIOS-reported values (MHz); use them as a fallback when
/// CPUID leaf 0x16 is unavailable.
/// </summary>
public sealed record SmbiosProcessorInfo(
    string? SocketDesignation,
    float? MaxSpeedMHz,
    float? ExternalClockMHz,
    int? LogicalCoreCount,
    CpuCacheInfo? CacheInfo);
