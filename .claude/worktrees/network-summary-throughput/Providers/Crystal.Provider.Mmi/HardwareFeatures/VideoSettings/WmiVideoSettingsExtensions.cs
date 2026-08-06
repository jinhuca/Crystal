using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.VideoSettings;
public static class WmiVideoSettingsExtensions {
  private const string WmiClassName = WmiVideoSettings.ClassName;

  public static async Task<IReadOnlyList<VideoSettingsMetrics>> ToSafeVideoSettingsMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance association data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<VideoSettingsMetrics>();
      }

      var results = new List<VideoSettingsMetrics>(instancesData.Count);

      // 2. Loop through every detected video controller/setting relationship instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new VideoSettingsMetrics(
          Element: GetStr(WmiVideoSettings.Element),
          Setting: GetStr(WmiVideoSettings.Setting)));
      }
      return results;
    }
    catch {
      return Array.Empty<VideoSettingsMetrics>();
    }
  }
}
