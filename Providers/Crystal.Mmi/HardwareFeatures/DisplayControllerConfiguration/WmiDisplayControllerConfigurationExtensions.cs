using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.DisplayControllerConfiguration;
public static class WmiDisplayControllerConfigurationExtensions {
  private const string WmiClassName = WmiDisplayControllerConfiguration.ClassName;

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
