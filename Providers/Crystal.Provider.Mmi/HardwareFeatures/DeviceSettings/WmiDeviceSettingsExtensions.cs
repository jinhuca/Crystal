using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.DeviceSettings;

/// <summary>
/// Provides extension methods for <see cref="IWmiHardwareProvider"/> to read device settings metrics from WMI.
/// </summary>
public static class WmiDeviceSettingsExtensions {
  /// <summary>
  /// The WMI class name for device settings metrics.
  /// </summary>
  private const string WmiClassName = WmiDeviceSettings.ClassName;

  /// <summary>
  /// Asynchronously retrieves device settings metrics from WMI using the provided hardware provider.
  /// </summary>
  /// <param name="provider">The WMI hardware provider.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>
  /// A task that represents the asynchronous operation and returns a list of device settings metrics.
  /// </returns>
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
