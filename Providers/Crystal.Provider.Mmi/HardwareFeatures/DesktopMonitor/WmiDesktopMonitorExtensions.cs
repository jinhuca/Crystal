using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.DesktopMonitor;

/// <summary>
/// Provides extension methods for <see cref="IWmiHardwareProvider"/> to fetch and convert WMI
/// </summary>
public static class WmiDesktopMonitorExtensions {
  /// <summary>
  /// The WMI class name for desktop monitor metrics.
  /// </summary>
  private const string WmiClassName = WmiDesktopMonitor.ClassName;

  /// <summary>
  /// Fetches and converts WMI desktop monitor metrics into a safe, null-tolerant list of <see cref="DesktopMonitorMetrics"/> instances.
  /// </summary>
  /// <param name="provider">The WMI hardware provider.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
  public static async Task<IReadOnlyList<DesktopMonitorMetrics>> ToSafeDesktopMonitorMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance data collection asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if(instancesData == null || instancesData.Count == 0) return Array.Empty<DesktopMonitorMetrics>();

      var results = new List<DesktopMonitorMetrics>(instancesData.Count);

      foreach(var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN LOOKUP CONDITIONAL WRAPPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String 
          ? v.AsString() : null;
        int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int 
          ? v.AsInt() : null;
        bool? GetBool(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Bool 
          ? v.AsBool() : null;
        DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime 
          ? v.AsDateTime() : null;
        ushort[]? GetUShortArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.UShortArray 
          ? v.AsUShortArray() : null;

        // --- INSTANTIATE SORTED EXTRACTED VALUES ---
        results.Add(new DesktopMonitorMetrics(
          Availability: (ushort?)GetInt(WmiDesktopMonitor.Availability),
          Bandwidth: (uint?)GetInt(WmiDesktopMonitor.Bandwidth),
          Caption: GetStr(WmiDesktopMonitor.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiDesktopMonitor.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiDesktopMonitor.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiDesktopMonitor.CreationClassName),
          Description: GetStr(WmiDesktopMonitor.Description),
          DeviceID: GetStr(WmiDesktopMonitor.DeviceID),
          DisplayType: (ushort?)GetInt(WmiDesktopMonitor.DisplayType),
          ErrorCleared: GetBool(WmiDesktopMonitor.ErrorCleared),
          ErrorDescription: GetStr(WmiDesktopMonitor.ErrorDescription),
          InstallationDate: GetDate(WmiDesktopMonitor.InstallationDate),
          IsLocked: GetBool(WmiDesktopMonitor.IsLocked),
          LastErrorCode: (uint?)GetInt(WmiDesktopMonitor.LastErrorCode),
          MonitorManufacturer: GetStr(WmiDesktopMonitor.MonitorManufacturer),
          MonitorType: GetStr(WmiDesktopMonitor.MonitorType),
          Name: GetStr(WmiDesktopMonitor.Name),
          PixelsPerXLogicalInch: (uint?)GetInt(WmiDesktopMonitor.PixelsPerXLogicalInch),
          PixelsPerYLogicalInch: (uint?)GetInt(WmiDesktopMonitor.PixelsPerYLogicalInch),
          PNPDeviceID: GetStr(WmiDesktopMonitor.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiDesktopMonitor.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiDesktopMonitor.PowerManagementSupported),
          ScreenHeight: (uint?)GetInt(WmiDesktopMonitor.ScreenHeight),
          ScreenWidth: (uint?)GetInt(WmiDesktopMonitor.ScreenWidth),
          Status: GetStr(WmiDesktopMonitor.Status),
          StatusInfo: (ushort?)GetInt(WmiDesktopMonitor.StatusInfo),
          SystemCreationClassName: GetStr(WmiDesktopMonitor.SystemCreationClassName),
          SystemName: GetStr(WmiDesktopMonitor.SystemName)
          ));
      }
      return results;
    }
    catch {
      return Array.Empty<DesktopMonitorMetrics>();
    }
  }
}
