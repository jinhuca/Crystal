using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.SoftwareFeatures.Thread;
public static class WmiThreadExtensions {
  private const string WmiClassName = WmiThread.ClassName;

  public static async Task<IReadOnlyList<ThreadMetrics>> ToSafeThreadMetricsAsync(
      this IWmiHardwareProvider provider,
      CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance runtime thread arrays asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) return Array.Empty<ThreadMetrics>();

      var results = new List<ThreadMetrics>(instancesData.Count);

      // 2. Loop through every single detected scheduled execution thread
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String 
          ? v.AsString() : null;
        int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int 
          ? v.AsInt() : null;
        DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime 
          ? v.AsDateTime() : null;
        ulong? GetULong(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.ULong 
          ? v.AsReadOnlyULong() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new ThreadMetrics(
          Caption: GetStr(WmiThread.Caption),
          CreationClassName: GetStr(WmiThread.CreationClassName),
          Description: GetStr(WmiThread.Description),
          ElapsedTime: GetULong(WmiThread.ElapsedTime),
          ExecutionState: (uint?)GetInt(WmiThread.ExecutionState),
          Handle: GetStr(WmiThread.Handle),
          InstallDate: GetDate(WmiThread.InstallationDate),
          KernelModeTime: GetULong(WmiThread.KernelModeTime),
          LastErrorCode: GetStr(WmiThread.LastErrorCode),
          Priority: (uint?)GetInt(WmiThread.Priority),
          ProcessCreationClassName: GetStr(WmiThread.ProcessCreationClassName),
          ProcessHandle: GetStr(WmiThread.ProcessHandle),
          StartAddress: (uint?)GetInt(WmiThread.StartAddress),
          Status: GetStr(WmiThread.Status),
          StatusInfo: (ushort?)GetInt(WmiThread.StatusInfo),
          SystemCreationClassName: GetStr(WmiThread.SystemCreationClassName),
          SystemName: GetStr(WmiThread.SystemName),
          ThreadState: (uint?)GetInt(WmiThread.ThreadState),
          ThreadWaitReason: (uint?)GetInt(WmiThread.ThreadWaitReason),
          UserModeTime: GetULong(WmiThread.UserModeTime)));
      }

      return results;
    }
    catch {
      return Array.Empty<ThreadMetrics>();
    }
  }
}
