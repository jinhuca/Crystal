using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.SoftwareFeatures.Process;

public static class WmiProcessExtensions {
  private const string WmiClassName = WmiProcess.ClassName;

  public static async Task<IReadOnlyList<ProcessMetrics>> ToSafeProcessMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance runtime process data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if(instancesData == null || instancesData.Count == 0) {
        return Array.Empty<ProcessMetrics>();
      }

      var results = new List<ProcessMetrics>(instancesData.Count);

      // 2. Loop through every single detected active memory process footprint
      foreach(var data in instancesData) {
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
        results.Add(new ProcessMetrics(
          Caption: GetStr(WmiProcess.Caption),
          CommandLine: GetStr(WmiProcess.CommandLine),
          CreationClassName: GetStr(WmiProcess.CreationClassName),
          CreationDate: GetDate(WmiProcess.CreationDate),
          Description: GetStr(WmiProcess.Description),
          ExecutablePath: GetStr(WmiProcess.ExecutablePath),
          ExecutionState: (uint?)GetInt(WmiProcess.ExecutionState),
          Handle: (uint?)GetInt(WmiProcess.Handle),
          HandleCount: (uint?)GetInt(WmiProcess.HandleCount),
          InstallDate: GetDate(WmiProcess.InstallationDate),
          KernelModeTime: GetULong(WmiProcess.KernelModeTime),
          MaximumWorkingSetSize: (uint?)GetInt(WmiProcess.MaximumWorkingSetSize),
          MinimumWorkingSetSize: (uint?)GetInt(WmiProcess.MinimumWorkingSetSize),
          Name: GetStr(WmiProcess.Name),
          OtherOperationCount: GetULong(WmiProcess.OtherOperationCount),
          OtherTransferCount: GetULong(WmiProcess.OtherTransferCount),
          PageFaults: (uint?)GetInt(WmiProcess.PageFaults),
          PageFileUsage: (uint?)GetInt(WmiProcess.PageFileUsage),
          ParentProcessId: (uint?)GetInt(WmiProcess.ParentProcessId),
          PeakPageFileUsage: (uint?)GetInt(WmiProcess.PeakPageFileUsage),
          PeakVirtualSize: GetULong(WmiProcess.PeakVirtualSize),
          PeakWorkingSetSize: GetULong(WmiProcess.PeakWorkingSetSize),
          Priority: (uint?)GetInt(WmiProcess.Priority),
          PrivatePageCount: GetULong(WmiProcess.PrivatePageCount),
          ProcessId: (uint?)GetInt(WmiProcess.ProcessId),
          ReadOperationCount: GetULong(WmiProcess.ReadOperationCount),
          ReadTransferCount: GetULong(WmiProcess.ReadTransferCount),
          SessionId: (uint?)GetInt(WmiProcess.SessionId),
          Status: GetStr(WmiProcess.Status),
          TerminationDate: GetDate(WmiProcess.TerminationDate),
          UserModeTime: GetULong(WmiProcess.UserModeTime),
          VirtualSize: GetULong(WmiProcess.VirtualSize),
          WindowsVersion: GetStr(WmiProcess.WindowsVersion),
          WorkingSetSize: GetULong(WmiProcess.WorkingSetSize),
          WriteOperationCount: GetULong(WmiProcess.WriteOperationCount),
          WriteTransferCount: GetULong(WmiProcess.WriteTransferCount)
        ));
      }

      return results;
    }
    catch {
      return Array.Empty<ProcessMetrics>();
    }
  }
}
