using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.SoftwareFeatures.QuickFixEngineering;
public static class WmiQuickFixEngineeringExtensions {
  private const string WmiClassName = WmiQuickFixEngineering.ClassName;

  public static async Task<IReadOnlyList<QuickFixEngineeringMetrics>> ToSafeQuickFixEngineeringMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance hotfix data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<QuickFixEngineeringMetrics>();
      }

      var results = new List<QuickFixEngineeringMetrics>(instancesData.Count);

      // 2. Loop through every detected hotfix instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;
        DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime
          ? v.AsDateTime() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new QuickFixEngineeringMetrics(
          Caption: GetStr(WmiQuickFixEngineering.Caption),
          CSName: GetStr(WmiQuickFixEngineering.CSName),
          Description: GetStr(WmiQuickFixEngineering.Description),
          FixComments: GetStr(WmiQuickFixEngineering.FixComments),
          HotFixID: GetStr(WmiQuickFixEngineering.HotFixID),
          InstallDate: GetDate(WmiQuickFixEngineering.InstallDate),
          InstalledBy: GetStr(WmiQuickFixEngineering.InstalledBy),
          InstalledOn: GetStr(WmiQuickFixEngineering.InstalledOn),
          Name: GetStr(WmiQuickFixEngineering.Name),
          ServicePackInEffect: GetStr(WmiQuickFixEngineering.ServicePackInEffect),
          Status: GetStr(WmiQuickFixEngineering.Status)));
      }
      return results;
    }
    catch {
      return Array.Empty<QuickFixEngineeringMetrics>();
    }
  }
}
