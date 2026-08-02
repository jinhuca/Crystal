using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.SoftwareFeatures.SystemDriver;
public static class WmiSystemDriverExtensions {
  private const string WmiClassName = WmiSystemDriver.ClassName;

  public static async Task<IReadOnlyList<SystemDriverMetrics>> ToSafeSystemDriverMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance system driver data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<SystemDriverMetrics>();
      }

      var results = new List<SystemDriverMetrics>(instancesData.Count);

      // 2. Loop through every detected system driver instance sequentially
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
        results.Add(new SystemDriverMetrics(
          AcceptPause: GetBool(WmiSystemDriver.AcceptPause),
          AcceptStop: GetBool(WmiSystemDriver.AcceptStop),
          Caption: GetStr(WmiSystemDriver.Caption),
          CreationClassName: GetStr(WmiSystemDriver.CreationClassName),
          Description: GetStr(WmiSystemDriver.Description),
          DesktopInteract: GetBool(WmiSystemDriver.DesktopInteract),
          DisplayName: GetStr(WmiSystemDriver.DisplayName),
          ErrorControl: GetStr(WmiSystemDriver.ErrorControl),
          ExitCode: (uint?)GetInt(WmiSystemDriver.ExitCode),
          InstallDate: GetDate(WmiSystemDriver.InstallationDate),
          Name: GetStr(WmiSystemDriver.Name),
          PathName: GetStr(WmiSystemDriver.PathName),
          ServiceSpecificExitCode: (uint?)GetInt(WmiSystemDriver.ServiceSpecificExitCode),
          ServiceType: GetStr(WmiSystemDriver.ServiceType),
          Started: GetBool(WmiSystemDriver.Started),
          StartMode: GetStr(WmiSystemDriver.StartMode),
          StartName: GetStr(WmiSystemDriver.StartName),
          State: GetStr(WmiSystemDriver.State),
          Status: GetStr(WmiSystemDriver.Status),
          SystemCreationClassName: GetStr(WmiSystemDriver.SystemCreationClassName),
          SystemName: GetStr(WmiSystemDriver.SystemName),
          TagId: (uint?)GetInt(WmiSystemDriver.TagId)));
      }
      return results;
    }
    catch {
      return Array.Empty<SystemDriverMetrics>();
    }
  }
}
