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

/// <summary>A live physical-memory reading: used percentage (0-100) plus used and available
/// capacity in GB (each nullable when the matching data sensor is unavailable).</summary>
public sealed record MemoryLoadReading(double LoadPercent, double? UsedGB, double? AvailableGB);
