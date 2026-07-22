using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.DeviceSettings;
public static class WmiDeviceSettingsExtensions {
  private const string WmiClassName = WmiDeviceSettings.ClassName;

  public static async Task<IReadOnlyList<DeviceSettingsMetrics>> ToSafeDeviceSettingsMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance association data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<DeviceSettingsMetrics>();
      }

      var results = new List<DeviceSettingsMetrics>(instancesData.Count);

      // 2. Loop through every detected device/setting relationship instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new DeviceSettingsMetrics(
          Element: GetStr(WmiDeviceSettings.Element),
          Setting: GetStr(WmiDeviceSettings.Setting)));
      }
      return results;
    }
    catch {
      return Array.Empty<DeviceSettingsMetrics>();
    }
  }
}
