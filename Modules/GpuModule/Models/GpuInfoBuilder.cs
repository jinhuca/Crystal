using Crystal.Provider.Mmi.HardwareFeatures.VideoController;
using Crystal.Provider.Mmi.MmiEngine;

namespace GpuModule.Models;

/// <summary>
/// Builds the static GPU adapter inventory from WMI (<c>Win32_VideoController</c>) and pairs
/// it with live core-load from <see cref="GpuLoadSource"/>. Adapters are classified as
/// integrated or dedicated so the view can render the two columns of the reference design.
/// </summary>
public sealed class GpuInfoBuilder {
  private readonly IWmiHardwareProvider _wmi;
  private readonly GpuLoadSource _loads;

  // Markers that identify CPU-integrated graphics by product/chip name. Anything else is
  // treated as a discrete card. This is a display heuristic, not a hardware guarantee.
  private static readonly string[] IntegratedMarkers =
      ["UHD Graphics", "HD Graphics", "Iris", "Radeon(TM) Graphics", "Radeon Graphics", "Vega", "AMD Radeon(TM)"];

  public GpuInfoBuilder(IWmiHardwareProvider wmi, GpuLoadSource loads) {
    _wmi = wmi;
    _loads = loads;
  }

  public async Task<GpuSnapshot> BuildAsync(CancellationToken ct) {
    var controllers = await _wmi.ToSafeVideoControllerMetricsAsync(ct);
    var adapters = controllers
        .Where(c => !string.IsNullOrWhiteSpace(c.Name))
        .Select(ToAdapter)
        .ToList();

    return new GpuSnapshot(adapters, _loads.Read());
  }

  private static GpuAdapterInfo ToAdapter(VideoControllerMetrics c) {
    var name = c.Name!;
    var kind = IntegratedMarkers.Any(m => name.Contains(m, StringComparison.OrdinalIgnoreCase))
        ? GpuKind.Integrated
        : GpuKind.Dedicated;

    return new GpuAdapterInfo(
        Name: name,
        Kind: kind,
        VideoRamGB: c.VideoRamInGB,
        DisplayMode: c.FormattedDisplayMode,
        DriverVersion: c.DriverVersion,
        VideoProcessor: c.VideoProcessor,
        RefreshRateHz: c.CurrentRefreshRate);
  }
}
