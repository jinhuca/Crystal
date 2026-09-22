namespace Crystal.Service.Memory;

/// <summary>
/// One physical RAM module (a populated slot). All identity fields are nullable because
/// SMBIOS/WMI leaves them blank on some boards or for generic/unbranded sticks.
/// </summary>
/// <param name="SlotLabel">The board's label for the slot the module sits in (e.g. "DIMM 0").</param>
/// <param name="CapacityGB">The module's capacity in GB, or null if WMI did not report it.</param>
/// <param name="SpeedMHz">The module's rated (SPD) speed in MHz.</param>
/// <param name="ConfiguredSpeedMHz">The speed the module is actually running at in MHz, which can be
/// lower than <paramref name="SpeedMHz"/> when the board underclocks it or XMP/EXPO is not applied.</param>
/// <param name="FormFactor">The physical form factor (e.g. "DIMM", "SODIMM").</param>
/// <param name="Manufacturer">The module manufacturer, or null if unknown.</param>
/// <param name="PartNumber">The manufacturer part number, or null if unknown.</param>
/// <param name="SerialNumber">The module serial number, or null if unknown.</param>
public record MemoryModuleInfo(
    string SlotLabel,
    double? CapacityGB,
    uint? SpeedMHz,
    uint? ConfiguredSpeedMHz,
    string FormFactor,
    string? Manufacturer,
    string? PartNumber,
    string? SerialNumber);

/// <summary>
/// The system's installed memory: the populated modules plus rolled-up totals. Includes
/// the memory technology (e.g. "DDR5"), the form factor of the populated slots, and the total
/// number of slots on the board (populated + empty) so the detail view can read "2 of 4".
/// </summary>
/// <param name="Modules">The populated modules, one entry per occupied slot.</param>
/// <param name="TotalCapacityGB">The summed capacity of all modules in GB.</param>
/// <param name="PopulatedSlots">The number of occupied slots (equal to <paramref name="Modules"/> count).</param>
/// <param name="MaxSpeedMHz">The highest rated speed across the populated modules in MHz.</param>
/// <param name="MemoryType">The memory technology (e.g. "DDR5"), read from the first populated stick.</param>
/// <param name="FormFactor">The form factor of the populated slots (e.g. "DIMM").</param>
/// <param name="TotalSlots">The board's total slot count (populated + empty), or null when the
/// memory-array WMI class reports nothing.</param>
public record MemorySnapshot(
    IReadOnlyList<MemoryModuleInfo> Modules,
    double? TotalCapacityGB,
    int PopulatedSlots,
    uint? MaxSpeedMHz,
    string? MemoryType = null,
    string? FormFactor = null,
    int? TotalSlots = null);

/// <summary>
/// A live memory reading. The physical-load fields come from the telemetry provider; the
/// remaining kernel-memory fields come from <c>GetPerformanceInfo</c> (all GB, each nullable when
/// the corresponding source is unavailable).
/// </summary>
/// <param name="LoadPercent">Percentage of installed RAM in use (0 when the load sensor is missing).</param>
/// <param name="UsedGB">Physical RAM in use, in GB.</param>
/// <param name="AvailableGB">Physical RAM available, in GB.</param>
/// <param name="CommittedGB">Current commit charge (RAM + pagefile backed), in GB.</param>
/// <param name="CommitLimitGB">Maximum commit charge the system can accept, in GB.</param>
/// <param name="CommitPeakGB">Highest commit charge since boot, in GB.</param>
/// <param name="CachedGB">System cache (approximates Task Manager's "Cached"), in GB.</param>
/// <param name="PagedPoolGB">Kernel paged pool size, in GB.</param>
/// <param name="NonPagedPoolGB">Kernel non-paged pool size, in GB.</param>
/// <param name="HardwareReservedGB">Installed RAM the OS cannot use (installed minus OS-usable), in GB.</param>
/// <param name="PhysicalTotalGB">OS-usable physical memory — the span the composition bar fills, in GB.</param>
/// <param name="ModifiedGB">Modified page-list size (composition segment), in GB.</param>
/// <param name="StandbyGB">Standby page-list size (composition segment), in GB.</param>
/// <param name="FreeGB">Free/zero page-list size (composition segment), in GB.</param>
/// <param name="PageFileUsedGB">Pagefile bytes in use, summed across all pagefiles, in GB.</param>
/// <param name="PageFileTotalGB">Total configured pagefile size, summed across all pagefiles, in GB.</param>
/// <param name="PageFilePeakGB">Highest pagefile occupancy since boot, in GB.</param>
public sealed record MemoryLoadReading(
    double LoadPercent,
    double? UsedGB,
    double? AvailableGB,
    double? CommittedGB = null,
    double? CommitLimitGB = null,
    double? CommitPeakGB = null,
    double? CachedGB = null,
    double? PagedPoolGB = null,
    double? NonPagedPoolGB = null,
    double? HardwareReservedGB = null,
    double? PhysicalTotalGB = null,
    double? ModifiedGB = null,
    double? StandbyGB = null,
    double? FreeGB = null,
    double? PageFileUsedGB = null,
    double? PageFileTotalGB = null,
    double? PageFilePeakGB = null);
