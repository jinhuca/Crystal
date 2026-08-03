using Crystal.Provider.Mmi.HardwareFeatures.PhysicalMemory;
using Crystal.Provider.Mmi.MmiEngine;

namespace MemoryModule.Models;

/// <summary>Builds the static memory inventory from WMI (<c>Win32_PhysicalMemory</c>),
/// mapping each populated slot and rolling up the system totals.</summary>
public sealed class MemoryInfoBuilder {
  private readonly IWmiHardwareProvider _wmi;

  public MemoryInfoBuilder(IWmiHardwareProvider wmi) => _wmi = wmi;

  public async Task<MemorySnapshot> BuildAsync(CancellationToken ct) {
    var sticks = await _wmi.ToSafePhysicalMemoryMetricsAsync(ct);
    var modules = sticks.Select(ToModule).ToList();

    return new MemorySnapshot(
        Modules: modules,
        TotalCapacityGB: modules.Sum(m => m.CapacityGB ?? 0),
        PopulatedSlots: modules.Count,
        MaxSpeedMHz: modules.Max(m => m.SpeedMHz));
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
