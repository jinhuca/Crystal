using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.SoftwareFeatures.GroupUser;
public static class WmiGroupUserExtensions {
  private const string WmiClassName = WmiGroupUser.ClassName;

  public static async Task<IReadOnlyList<GroupUserMetrics>> ToSafeGroupUserMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance group-membership data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<GroupUserMetrics>();
      }

      var results = new List<GroupUserMetrics>(instancesData.Count);

      // 2. Loop through every detected group-membership instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new GroupUserMetrics(
          GroupComponent: GetStr(WmiGroupUser.GroupComponent),
          PartComponent: GetStr(WmiGroupUser.PartComponent)));
      }
      return results;
    }
    catch {
      return Array.Empty<GroupUserMetrics>();
    }
  }
}
