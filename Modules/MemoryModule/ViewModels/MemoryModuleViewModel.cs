using Crystal.Service.Memory;

namespace MemoryModule.ViewModels;

/// <summary>One populated RAM slot in the detail list.</summary>
public sealed class MemoryModuleViewModel {
  public MemoryModuleViewModel(MemoryModuleInfo info) {
    SlotLabel = info.SlotLabel;
    CapacityLabel = info.CapacityGB is { } gb ? $"{gb:0.#} GB" : "—";
    SpeedLabel = info.SpeedMHz is { } s ? $"{s} MHz" : "—";
    ConfiguredSpeedLabel = info.ConfiguredSpeedMHz is { } cs ? $"{cs} MHz" : "—";
    FormFactor = info.FormFactor;
    Manufacturer = string.IsNullOrWhiteSpace(info.Manufacturer) ? "—" : info.Manufacturer!;
    PartNumber = string.IsNullOrWhiteSpace(info.PartNumber) ? "—" : info.PartNumber!;
    SerialNumber = string.IsNullOrWhiteSpace(info.SerialNumber) ? "—" : info.SerialNumber!;
  }

  public string SlotLabel { get; }
  public string CapacityLabel { get; }
  public string SpeedLabel { get; }
  public string ConfiguredSpeedLabel { get; }
  public string FormFactor { get; }
  public string Manufacturer { get; }
  public string PartNumber { get; }
  public string SerialNumber { get; }
}
