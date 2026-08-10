using Crystal.Provider.Mmi.HardwareFeatures.PhysicalMemory;
using Crystal.Provider.Mmi.HardwareFeatures.PhysicalMemoryArray;
using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Service.Memory;

/// <summary>Builds the static memory inventory from WMI (<c>Win32_PhysicalMemory</c> for the
/// populated sticks and <c>Win32_PhysicalMemoryArray</c> for the board's total slot count),
/// mapping each populated slot and rolling up the system totals.</summary>
public sealed class MemoryInfoBuilder {
  private readonly IWmiHardwareProvider _wmi;

  public MemoryInfoBuilder(IWmiHardwareProvider wmi) => _wmi = wmi;

  public async Task<MemorySnapshot> BuildAsync(CancellationToken ct) {
    var sticks = await _wmi.ToSafePhysicalMemoryMetricsAsync(ct);
    var arrays = await _wmi.ToSafePhysicalMemoryArrayMetricsAsync(ct);
    var modules = sticks.Select(ToModule).ToList();

    // The board's total slot count is the sum of MemoryDevices across the memory arrays; fall back
    // to the populated count when the array class reports nothing.
    int? totalSlots = arrays.Count > 0
        ? arrays.Sum(a => a.MemoryDevices ?? 0)
        : null;
    if (totalSlots is 0) totalSlots = null;

    return new MemorySnapshot(
        Modules: modules,
        TotalCapacityGB: modules.Sum(m => m.CapacityGB ?? 0),
        PopulatedSlots: modules.Count,
        MaxSpeedMHz: modules.Max(m => m.SpeedMHz),
        // DDR type and form factor are uniform across sticks in practice; read the first populated.
        MemoryType: sticks.Select(s => s.MemoryTypeName).FirstOrDefault(t => t is not null),
        FormFactor: sticks.FirstOrDefault()?.FormFactorName,
        TotalSlots: totalSlots);
  }

  private static MemoryModuleInfo ToModule(PhysicalMemoryMetrics m) => new(
      SlotLabel: m.DeviceLocator ?? m.BankLabel ?? "Unknown slot",
      CapacityGB: m.CapacityInGB,
      SpeedMHz: m.Speed,
      ConfiguredSpeedMHz: m.ConfiguredClockSpeed,
      FormFactor: m.FormFactorName,
      Manufacturer: m.Manufacturer,
      PartNumber: m.PartNumber,
      SerialNumber: m.SerialNumber);
}
