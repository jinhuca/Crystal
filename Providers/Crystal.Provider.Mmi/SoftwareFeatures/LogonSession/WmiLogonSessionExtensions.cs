using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.SoftwareFeatures.LogonSession;
public static class WmiLogonSessionExtensions {
  private const string WmiClassName = WmiLogonSession.ClassName;

  public static async Task<IReadOnlyList<LogonSessionMetrics>> ToSafeLogonSessionMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance logon session data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<LogonSessionMetrics>();
      }

      var results = new List<LogonSessionMetrics>(instancesData.Count);

      // 2. Loop through every detected logon session instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;
        int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int
          ? v.AsInt() : null;
        DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime
          ? v.AsDateTime() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new LogonSessionMetrics(
          AuthenticationPackage: GetStr(WmiLogonSession.AuthenticationPackage),
          Caption: GetStr(WmiLogonSession.Caption),
          Description: GetStr(WmiLogonSession.Description),
          InstallDate: GetDate(WmiLogonSession.InstallDate),
          LogonId: GetStr(WmiLogonSession.LogonId),
          LogonType: (uint?)GetInt(WmiLogonSession.LogonType),
          Name: GetStr(WmiLogonSession.Name),
          StartTime: GetDate(WmiLogonSession.StartTime),
          Status: GetStr(WmiLogonSession.Status)));
      }
      return results;
    }
    catch {
      return Array.Empty<LogonSessionMetrics>();
    }
  }
}
