using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.SoftwareFeatures.Service;
public static class WmiServiceExtensions {
  private const string WmiClassName = WmiService.ClassName;

  public static async Task<IReadOnlyList<ServiceMetrics>> ToSafeServiceMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance runtime service data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<ServiceMetrics>();
      }

      var results = new List<ServiceMetrics>(instancesData.Count);

      // 2. Loop through every single detected active background service
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
        results.Add(new ServiceMetrics(
          AcceptPause: GetBool(WmiService.AcceptPause),
          AcceptStop: GetBool(WmiService.AcceptStop),
          Caption: GetStr(WmiService.Caption),
          CreationClassName: GetStr(WmiService.CreationClassName),
          Description: GetStr(WmiService.Description),
          DesktopInteract: GetBool(WmiService.DesktopInteract),
          DisplayName: GetStr(WmiService.DisplayName),
          ErrorControl: GetStr(WmiService.ErrorControl),
          ExitCode: (uint?)GetInt(WmiService.ExitCode),
          InstallDate: GetDate(WmiService.InstallationDate),
          Name: GetStr(WmiService.Name),
          PathName: GetStr(WmiService.PathName),
          ProcessId: (uint?)GetInt(WmiService.ProcessId),
          ServiceSpecificExitCode: (uint?)GetInt(WmiService.ServiceSpecificExitCode),
          ServiceType: GetStr(WmiService.ServiceType),
          Started: GetBool(WmiService.Started),
          StartMode: GetStr(WmiService.StartMode),
          StartName: GetStr(WmiService.StartName),
          Status: GetStr(WmiService.Status),
          StatusInfo: (ushort?)GetInt(WmiService.StatusInfo),
          SystemCreationClassName: GetStr(WmiService.SystemCreationClassName),
          SystemName: GetStr(WmiService.SystemName),
          TagId: (uint?)GetInt(WmiService.TagId),
          State: GetStr(WmiService.State)));
      }
      return results;
    }
    catch {
      return Array.Empty<ServiceMetrics>();
    }
  }
}
