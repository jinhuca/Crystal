using Crystal.Provider.Mmi.HardwareFeatures.VideoController;
using Crystal.Provider.Mmi.MmiEngine;
using Microsoft.Win32;

namespace Crystal.Service.Gpu;

/// <summary>
/// Builds the static GPU adapter inventory from WMI (<c>Win32_VideoController</c>) and pairs
/// it with live core-load from <see cref="GpuLoadSource"/>. Adapters are classified as
/// integrated or dedicated so the view can render the two columns of the reference design.
/// </summary>
public sealed class GpuInfoBuilder {
  private readonly IWmiHardwareProvider _wmi;
  private readonly IGpuLoadSource _loads;

  // Markers that identify CPU-integrated graphics by product/chip name. Anything else is
  // treated as a discrete card. This is a display heuristic, not a hardware guarantee.
  private static readonly string[] IntegratedMarkers =
      ["UHD Graphics", "HD Graphics", "Iris", "Radeon(TM) Graphics", "Radeon Graphics", "Vega", "AMD Radeon(TM)"];

  public GpuInfoBuilder(IWmiHardwareProvider wmi, IGpuLoadSource loads) {
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
        DriverDate: ReadDriverDate(c.PNPDeviceID) ?? c.InfDate ?? c.InstallDate,
        VideoProcessor: c.VideoProcessor,
        PhysicalLocation: ReadPhysicalLocation(c.PNPDeviceID),
        RefreshRateHz: c.CurrentRefreshRate);
  }

  // Win32_VideoController.InfDate/InstallDate are almost always empty, so the "Driver Date" field
  // reads blank. The reliable value lives in the driver's class key: the device's PnP enumeration
  // key holds a "Driver" value (a relative path like "{4d36e968-...}\0000") pointing under
  // Control\Class, where "DriverDate" is stored as a "M-D-YYYY" string. We resolve that here.
  private static DateTime? ReadDriverDate(string? pnpDeviceId) {
    if (string.IsNullOrWhiteSpace(pnpDeviceId)) return null;
    try {
      using var enumKey = Registry.LocalMachine.OpenSubKey(
          $@"SYSTEM\CurrentControlSet\Enum\{pnpDeviceId}");
      if (enumKey?.GetValue("Driver") is not string driverPath || string.IsNullOrWhiteSpace(driverPath))
        return null;

      using var classKey = Registry.LocalMachine.OpenSubKey(
          $@"SYSTEM\CurrentControlSet\Control\Class\{driverPath}");
      if (classKey?.GetValue("DriverDate") is not string raw || string.IsNullOrWhiteSpace(raw))
        return null;

      return DateTime.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture,
          System.Globalization.DateTimeStyles.None, out var date) ? date : null;
    } catch {
      return null;
    }
  }

  // The "PCI bus X, device Y, function Z" string isn't a Win32_VideoController property; it's the
  // device's LocationInformation, stored in the registry under its PnP enumeration key. Windows
  // stores it as an indirect resource reference, e.g.
  //   "@System32\drivers\pci.sys,#65536;PCI bus %1, device %2, function %3;(11,0,0)"
  // The trailing segments carry everything we need: a "%1/%2/%3" template plus a "(11,0,0)" arg
  // tuple. We parse those directly rather than calling SHLoadIndirectString, whose relative
  // resource path (no %SystemRoot%) fails to load on many systems and yields an empty string.
  private static string? ReadPhysicalLocation(string? pnpDeviceId) {
    if (string.IsNullOrWhiteSpace(pnpDeviceId)) return null;
    try {
      using var key = Registry.LocalMachine.OpenSubKey(
          $@"SYSTEM\CurrentControlSet\Enum\{pnpDeviceId}");
      if (key?.GetValue("LocationInformation") is not string raw || string.IsNullOrWhiteSpace(raw))
        return null;

      // Plain (non-indirect) values are already the final text.
      if (!raw.StartsWith('@')) return raw;

      var segments = raw.Split(';');
      var template = Array.Find(segments, s => s.Contains("%1"));
      var args = Array.Find(segments, s => s.StartsWith('(') && s.EndsWith(')'));
      if (template is null || args is null) return null;

      var values = args.Trim('(', ')').Split(',');
      var result = template;
      for (int i = 0; i < values.Length; i++)
        result = result.Replace($"%{i + 1}", values[i].Trim());
      return result;
    } catch {
      return null;
    }
  }
}
