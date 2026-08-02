using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.SoftwareFeatures.StartupCommand;
public static class WmiStartupCommandExtensions {
  private const string WmiClassName = WmiStartupCommand.ClassName;

  public static async Task<IReadOnlyList<StartupCommandMetrics>> ToSafeStartupCommandMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance startup command data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<StartupCommandMetrics>();
      }

      var results = new List<StartupCommandMetrics>(instancesData.Count);

      // 2. Loop through every detected startup command instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new StartupCommandMetrics(
          Caption: GetStr(WmiStartupCommand.Caption),
          Command: GetStr(WmiStartupCommand.Command),
          Description: GetStr(WmiStartupCommand.Description),
          Location: GetStr(WmiStartupCommand.Location),
          Name: GetStr(WmiStartupCommand.Name),
          SettingID: GetStr(WmiStartupCommand.SettingID),
          User: GetStr(WmiStartupCommand.User),
          UserSID: GetStr(WmiStartupCommand.UserSID)));
      }
      return results;
    }
    catch {
      return Array.Empty<StartupCommandMetrics>();
    }
  }
}
