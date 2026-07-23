using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.SoftwareFeatures.Environment;
public static class WmiEnvironmentExtensions {
  private const string WmiClassName = WmiEnvironment.ClassName;

  public static async Task<IReadOnlyList<EnvironmentMetrics>> ToSafeEnvironmentMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance environment variable data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<EnvironmentMetrics>();
      }

      var results = new List<EnvironmentMetrics>(instancesData.Count);

      // 2. Loop through every detected environment variable instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;
        bool? GetBool(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Bool
          ? v.AsBool() : null;
        DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime
          ? v.AsDateTime() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new EnvironmentMetrics(
          Caption: GetStr(WmiEnvironment.Caption),
          Description: GetStr(WmiEnvironment.Description),
          InstallDate: GetDate(WmiEnvironment.InstallDate),
          Name: GetStr(WmiEnvironment.Name),
          Status: GetStr(WmiEnvironment.Status),
          SystemVariable: GetBool(WmiEnvironment.SystemVariable),
          UserName: GetStr(WmiEnvironment.UserName),
          VariableValue: GetStr(WmiEnvironment.VariableValue)));
      }
      return results;
    }
    catch {
      return Array.Empty<EnvironmentMetrics>();
    }
  }
}
