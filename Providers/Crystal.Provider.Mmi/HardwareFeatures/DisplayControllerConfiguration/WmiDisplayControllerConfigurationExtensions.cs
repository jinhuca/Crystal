using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.DisplayControllerConfiguration;

/// <summary>
/// Provides extension methods for <see cref="IWmiHardwareProvider"/> to read display controller configuration metrics 
/// from WMI (<c>Win32_DisplayControllerConfiguration</c>).
/// </summary>
public static class WmiDisplayControllerConfigurationExtensions {
  /// <summary>
  /// The WMI class name for display controller configuration metrics.
  /// </summary>
  private const string WmiClassName = WmiDisplayControllerConfiguration.ClassName;

  /// <summary>
  /// Asynchronously retrieves display controller configuration metrics from WMI and maps them to a list of 
  /// <see cref="DisplayControllerConfigurationMetrics"/> objects.
  /// </summary>
  /// <param name="provider">The WMI hardware provider.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>A task that represents the asynchronous operation and returns a list of display controller configuration metrics.</returns>
  public static async Task<IReadOnlyList<DisplayControllerConfigurationMetrics>> ToSafeDisplayControllerConfigurationMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance display controller configuration data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<DisplayControllerConfigurationMetrics>();
      }

      var results = new List<DisplayControllerConfigurationMetrics>(instancesData.Count);

      // 2. Loop through every detected display controller configuration instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;
        int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int
          ? v.AsInt() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new DisplayControllerConfigurationMetrics(
          BitsPerPixel: (uint?)GetInt(WmiDisplayControllerConfiguration.BitsPerPixel),
          Caption: GetStr(WmiDisplayControllerConfiguration.Caption),
          ColorPlanes: (uint?)GetInt(WmiDisplayControllerConfiguration.ColorPlanes),
          Description: GetStr(WmiDisplayControllerConfiguration.Description),
          DeviceEntriesInAColorTable: (uint?)GetInt(WmiDisplayControllerConfiguration.DeviceEntriesInAColorTable),
          DeviceSpecificPens: (uint?)GetInt(WmiDisplayControllerConfiguration.DeviceSpecificPens),
          HorizontalResolution: (uint?)GetInt(WmiDisplayControllerConfiguration.HorizontalResolution),
          Name: GetStr(WmiDisplayControllerConfiguration.Name),
          RefreshRate: GetInt(WmiDisplayControllerConfiguration.RefreshRate),
          ReservedSystemPaletteEntries: (uint?)GetInt(WmiDisplayControllerConfiguration.ReservedSystemPaletteEntries),
          SettingID: GetStr(WmiDisplayControllerConfiguration.SettingID),
          SystemPaletteEntries: (uint?)GetInt(WmiDisplayControllerConfiguration.SystemPaletteEntries),
          VerticalResolution: (uint?)GetInt(WmiDisplayControllerConfiguration.VerticalResolution),
          VideoMode: GetStr(WmiDisplayControllerConfiguration.VideoMode)));
      }
      return results;
    }
    catch {
      return Array.Empty<DisplayControllerConfigurationMetrics>();
    }
  }
}
