using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.SoftwareFeatures.Group;
public static class WmiGroupExtensions {
  private const string WmiClassName = WmiGroup.ClassName;

  public static async Task<IReadOnlyList<GroupMetrics>> ToSafeGroupMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance group data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<GroupMetrics>();
      }

      var results = new List<GroupMetrics>(instancesData.Count);

      // 2. Loop through every detected group instance sequentially
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
        results.Add(new GroupMetrics(
          Caption: GetStr(WmiGroup.Caption),
          Description: GetStr(WmiGroup.Description),
          Domain: GetStr(WmiGroup.Domain),
          InstallDate: GetDate(WmiGroup.InstallDate),
          LocalAccount: GetBool(WmiGroup.LocalAccount),
          Name: GetStr(WmiGroup.Name),
          SID: GetStr(WmiGroup.SID),
          SIDType: (byte?)GetInt(WmiGroup.SIDType),
          Status: GetStr(WmiGroup.Status)));
      }
      return results;
    }
    catch {
      return Array.Empty<GroupMetrics>();
    }
  }
}
