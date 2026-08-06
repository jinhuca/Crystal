using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.ParallelPort;
public static class WmiParallelPortExtensions {
  private const string WmiClassName = WmiParallelPort.ClassName;

  public static async Task<IReadOnlyList<ParallelPortMetrics>> ToSafeParallelPortMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance parallel port data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<ParallelPortMetrics>();
      }

      var results = new List<ParallelPortMetrics>(instancesData.Count);

      // 2. Loop through every detected parallel port instance sequentially
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
        ushort[]? GetUShortArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.UShortArray
          ? v.AsUShortArray() : null;
        string? GetFlattenedStrArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.StringArray
          ? string.Join(", ", v.AsStringArray() ?? Array.Empty<string>()) : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new ParallelPortMetrics(
          Availability: (ushort?)GetInt(WmiParallelPort.Availability),
          Capabilities: GetUShortArr(WmiParallelPort.Capabilities),
          CapabilityDescriptions: GetFlattenedStrArr(WmiParallelPort.CapabilityDescriptions),
          Caption: GetStr(WmiParallelPort.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiParallelPort.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiParallelPort.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiParallelPort.CreationClassName),
          Description: GetStr(WmiParallelPort.Description),
          DeviceID: GetStr(WmiParallelPort.DeviceID),
          DMASupport: GetBool(WmiParallelPort.DMASupport),
          ErrorCleared: GetBool(WmiParallelPort.ErrorCleared),
          ErrorDescription: GetStr(WmiParallelPort.ErrorDescription),
          InstallDate: GetDate(WmiParallelPort.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiParallelPort.LastErrorCode),
          MaxNumberControlled: (uint?)GetInt(WmiParallelPort.MaxNumberControlled),
          Name: GetStr(WmiParallelPort.Name),
          OSAutoDiscovered: GetBool(WmiParallelPort.OSAutoDiscovered),
          PNPDeviceID: GetStr(WmiParallelPort.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiParallelPort.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiParallelPort.PowerManagementSupported),
          ProtocolSupported: (ushort?)GetInt(WmiParallelPort.ProtocolSupported),
          Status: GetStr(WmiParallelPort.Status),
          StatusInfo: (ushort?)GetInt(WmiParallelPort.StatusInfo),
          SystemCreationClassName: GetStr(WmiParallelPort.SystemCreationClassName),
          SystemName: GetStr(WmiParallelPort.SystemName),
          TimeOfLastReset: GetDate(WmiParallelPort.TimeOfLastReset)));
      }
      return results;
    }
    catch {
      return Array.Empty<ParallelPortMetrics>();
    }
  }
}
