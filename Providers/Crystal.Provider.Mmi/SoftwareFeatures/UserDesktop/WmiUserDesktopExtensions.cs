using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.SoftwareFeatures.UserDesktop;
public static class WmiUserDesktopExtensions {
  private const string WmiClassName = WmiUserDesktop.ClassName;

  public static async Task<IReadOnlyList<UserDesktopMetrics>> ToSafeUserDesktopMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance user/desktop relationship data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<UserDesktopMetrics>();
      }

      var results = new List<UserDesktopMetrics>(instancesData.Count);

      // 2. Loop through every detected relationship instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new UserDesktopMetrics(
          Element: GetStr(WmiUserDesktop.Element),
          Setting: GetStr(WmiUserDesktop.Setting)));
      }
      return results;
    }
    catch {
      return Array.Empty<UserDesktopMetrics>();
    }
  }
}
