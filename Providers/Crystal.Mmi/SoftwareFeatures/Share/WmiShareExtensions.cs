using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.SoftwareFeatures.Share;
public static class WmiShareExtensions {
  private const string WmiClassName = WmiShare.ClassName;

  public static async Task<IReadOnlyList<ShareMetrics>> ToSafeShareMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance share data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<ShareMetrics>();
      }

      var results = new List<ShareMetrics>(instancesData.Count);

      // 2. Loop through every detected share instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;
        int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int
          ? v.AsInt() : null;
        bool? GetBool(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Bool
          ? v.AsBool() : null;
        DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime
          ? v.AsDateTime() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new ShareMetrics(
          AccessMask: (uint?)GetInt(WmiShare.AccessMask),
          AllowMaximum: GetBool(WmiShare.AllowMaximum),
          Caption: GetStr(WmiShare.Caption),
          Description: GetStr(WmiShare.Description),
          InstallDate: GetDate(WmiShare.InstallDate),
          MaximumAllowed: (uint?)GetInt(WmiShare.MaximumAllowed),
          Name: GetStr(WmiShare.Name),
          Path: GetStr(WmiShare.Path),
          Status: GetStr(WmiShare.Status),
          Type: (uint?)GetInt(WmiShare.Type)));
      }
      return results;
    }
    catch {
      return Array.Empty<ShareMetrics>();
    }
  }
}
