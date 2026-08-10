namespace Crystal.Service.Memory;

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

/// <summary>The system's installed memory: the populated modules plus rolled-up totals. Includes
/// the memory technology (e.g. "DDR5"), the form factor of the populated slots, and the total
/// number of slots on the board (populated + empty) so the detail view can read "2 of 4".</summary>
public record MemorySnapshot(
    IReadOnlyList<MemoryModuleInfo> Modules,
    double? TotalCapacityGB,
    int PopulatedSlots,
    uint? MaxSpeedMHz,
    string? MemoryType = null,
    string? FormFactor = null,
    int? TotalSlots = null);

/// <summary>A live memory reading. The physical-load fields come from the telemetry provider; the
/// remaining kernel-memory fields come from <c>GetPerformanceInfo</c> (all GB, each nullable when
/// the corresponding source is unavailable).</summary>
public sealed record MemoryLoadReading(
    double LoadPercent,
    double? UsedGB,
    double? AvailableGB,
    double? CommittedGB = null,
    double? CommitLimitGB = null,
    double? CachedGB = null,
    double? PagedPoolGB = null,
    double? NonPagedPoolGB = null,
    double? HardwareReservedGB = null);
