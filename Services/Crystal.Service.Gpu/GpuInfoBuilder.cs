using Crystal.Provider.Mmi.HardwareFeatures.VideoController;
using Crystal.Provider.Mmi.MmiEngine;
using Microsoft.Win32;

namespace Crystal.Service.Gpu;

/// <summary>
/// Builds the static GPU adapter inventory from WMI (<c>Win32_VideoController</c>) and pairs
/// it with live core-load from <see cref="GpuLoadSource"/>. Adapters are classified as
/// integrated or dedicated so the view can render the two columns of the reference design.
/// </summary>
public sealed class GpuInfoBuilder(IWmiHardwareProvider wmi, IGpuLoadSource loads) {

  /// <summary>
  /// The WMI provider that reads <c>Win32_VideoController</c> and returns a safe, null-tolerant
  /// snapshot of the adapter inventory.
  /// </summary>
  private readonly IWmiHardwareProvider _wmi = wmi;

  /// <summary>
  /// The source of live GPU core-load readings, which are paired with the static adapter inventory
  /// </summary>
  private readonly IGpuLoadSource _loads = loads;

  /// <summary>
  /// Markers that identify CPU-integrated graphics by product/chip name. Anything else is
  /// treated as a discrete card.
  /// </summary>
  private static readonly string[] IntegratedMarkers =
    ["UHD Graphics", "HD Graphics", "Iris", "Radeon(TM) Graphics", "Radeon Graphics", "Vega", "AMD Radeon(TM)"];

  /// <summary>
  /// Builds a <see cref="GpuSnapshot"/> by reading the static adapter inventory from WMI and
  /// pairing it with live core-load readings.
  /// </summary>
  /// <param name="ct">The cancellation token.</param>
  /// <returns>The built GPU snapshot.</returns>
  public async Task<GpuSnapshot> BuildAsync(CancellationToken ct) {
    var controllers = await _wmi.ToSafeVideoControllerMetricsAsync(ct);
    var adapters = controllers
      .Where(c => !string.IsNullOrWhiteSpace(c.Name))
      .Select(ToAdapter)
      .ToList();
    return new GpuSnapshot(adapters, _loads.Read());
  }

  /// <summary>
  /// Converts a <see cref="VideoControllerMetrics"/> to a <see cref="GpuAdapterInfo"/>, classifying
  /// it as integrated or dedicated based on its name.
  /// </summary>
  /// <param name="c">The video controller metrics.</param>
  /// <returns>The GPU adapter info.</returns>
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

  /// <summary>
  /// Win32_VideoController.InfDate/InstallDate are almost always empty, so the "Driver Date" field
  /// reads blank. The reliable value lives in the driver's class key: the device's PnP enumeration
  /// key holds a "Driver" value (a relative path like "{4d36e968-...}\0000") pointing under
  /// Control\Class, where "DriverDate" is stored as a "M-D-YYYY" string. We resolve that here.
  /// </summary>
  /// <param name="pnpDeviceId">The PnP device ID.</param>
  /// <returns>The driver date, or null if not found.</returns>
  private static DateTime? ReadDriverDate(string? pnpDeviceId) {
    if (string.IsNullOrWhiteSpace(pnpDeviceId)) {
      return null;
    }

    try {
      using var enumKey = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Enum\{pnpDeviceId}");
      if (enumKey?.GetValue("Driver") is not string driverPath || string.IsNullOrWhiteSpace(driverPath)) {
        return null;
      }

      using var classKey = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Control\Class\{driverPath}");
      return classKey?.GetValue("DriverDate") is not string raw || string.IsNullOrWhiteSpace(raw)
        ? null
        : DateTime.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture,
          System.Globalization.DateTimeStyles.None, out var date) ? date : null;
    }
    catch {
      return null;
    }
  }

  /// <summary>
  /// The "PCI bus X, device Y, function Z" string isn't a Win32_VideoController property; it's the
  /// device's LocationInformation, stored in the registry under its PnP enumeration key. Windows
  /// stores it as an indirect resource reference, e.g. "@System32\drivers\pci.sys,#65536;PCI bus %1, device %2, function %3;(11,0,0)"
  /// The trailing segments carry everything we need: a "%1/%2/%3" template plus a "(11,0,0)" arg tuple. 
  /// We parse those directly rather than calling SHLoadIndirectString, whose relative resource path (no %SystemRoot%) 
  /// fails to load on many systems and yields an empty string.
  /// </summary>
  /// <param name="pnpDeviceId">The PnP device ID.</param>
  /// <returns>The physical location, or null if not found.</returns>
  private static string? ReadPhysicalLocation(string? pnpDeviceId) {
    if (string.IsNullOrWhiteSpace(pnpDeviceId)) {
      return null;
    }

    try {
      using var key = Registry.LocalMachine.OpenSubKey(name: $@"SYSTEM\CurrentControlSet\Enum\{pnpDeviceId}");
      if (key?.GetValue("LocationInformation") is not string raw || string.IsNullOrWhiteSpace(raw)) {
        return null;
      }

      // Plain (non-indirect) values are already the final text.
      if (!raw.StartsWith('@')) {
        return raw;
      }

      var segments = raw.Split(';');
      var template = Array.Find(segments, s => s.Contains("%1"));
      var args = Array.Find(segments, s => s.StartsWith('(') && s.EndsWith(')'));
      if (template is null || args is null) {
        return null;
      }

      var values = args.Trim('(', ')').Split(',');
      var result = template;
      for (int i = 0; i < values.Length; i++)
        result = result.Replace($"%{i + 1}", values[i].Trim());
      return result;
    }
    catch {
      return null;
    }
  }
}
