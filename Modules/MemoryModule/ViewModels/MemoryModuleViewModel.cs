using Crystal.Service.Memory;

namespace MemoryModule.ViewModels;

/// <summary>One populated RAM slot in the detail list.</summary>
public sealed class MemoryModuleViewModel {
  public MemoryModuleViewModel(MemoryModuleInfo info) {
    SlotLabel = info.SlotLabel;
    CapacityLabel = info.CapacityGB is { } gb ? $"{gb:0.#} GB" : "—";
    SpeedLabel = FormatSpeed(info.SpeedMHz, info.ConfiguredSpeedMHz);
    FormFactor = info.FormFactor;
    Manufacturer = string.IsNullOrWhiteSpace(info.Manufacturer) ? "—" : info.Manufacturer!;
    PartNumber = string.IsNullOrWhiteSpace(info.PartNumber) ? "—" : info.PartNumber!;
    SerialNumber = string.IsNullOrWhiteSpace(info.SerialNumber) ? "—" : info.SerialNumber!;
  }

  public string SlotLabel { get; }
  public string CapacityLabel { get; }

  /// <summary>Rated speed, with the running (configured) speed in parentheses when it differs —
  /// e.g. "6000 MHz (running 4800)" reveals a stick not clocked to its XMP/EXPO rating.</summary>
  public string SpeedLabel { get; }
  public string FormFactor { get; }
  public string Manufacturer { get; }
  public string PartNumber { get; }
  public string SerialNumber { get; }

  private static string FormatSpeed(uint? ratedMHz, uint? configuredMHz) => (ratedMHz, configuredMHz) switch {
    ({ } rated, { } configured) when configured != rated => $"{rated} MHz (running {configured})",
    ({ } rated, _) => $"{rated} MHz",
    (null, { } configured) => $"{configured} MHz",
    _ => "—",
  };
}
