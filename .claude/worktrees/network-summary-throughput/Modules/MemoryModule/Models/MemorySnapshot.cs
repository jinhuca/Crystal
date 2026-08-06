namespace MemoryModule.Models;

/// <summary>One physical RAM module (a populated slot).</summary>
public record MemoryModuleInfo(
    string SlotLabel,
    double? CapacityGB,
    uint? SpeedMHz,
    uint? ConfiguredSpeedMHz,
    string FormFactor,
    string? Manufacturer,
    string? PartNumber,
    string? SerialNumber);

/// <summary>The system's installed memory: the populated modules plus rolled-up totals.</summary>
public record MemorySnapshot(
    IReadOnlyList<MemoryModuleInfo> Modules,
    double? TotalCapacityGB,
    int PopulatedSlots,
    uint? MaxSpeedMHz);
