using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.CurrentProbe;
public static class WmiCurrentProbeExtensions {
  private const string WmiClassName = WmiCurrentProbe.ClassName;

  public static async Task<IReadOnlyList<CurrentProbeMetrics>> ToSafeCurrentProbeMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance current probe data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<CurrentProbeMetrics>();
      }

      var results = new List<CurrentProbeMetrics>(instancesData.Count);

      // 2. Loop through every detected current probe instance sequentially
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

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new CurrentProbeMetrics(
          Accuracy: GetInt(WmiCurrentProbe.Accuracy),
          Availability: (ushort?)GetInt(WmiCurrentProbe.Availability),
          Caption: GetStr(WmiCurrentProbe.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiCurrentProbe.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiCurrentProbe.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiCurrentProbe.CreationClassName),
          CurrentReading: GetInt(WmiCurrentProbe.CurrentReading),
          Description: GetStr(WmiCurrentProbe.Description),
          DeviceID: GetStr(WmiCurrentProbe.DeviceID),
          ErrorCleared: GetBool(WmiCurrentProbe.ErrorCleared),
          ErrorDescription: GetStr(WmiCurrentProbe.ErrorDescription),
          InstallDate: GetDate(WmiCurrentProbe.InstallDate),
          IsLinear: GetBool(WmiCurrentProbe.IsLinear),
          LastErrorCode: (uint?)GetInt(WmiCurrentProbe.LastErrorCode),
          LowerThresholdCritical: GetInt(WmiCurrentProbe.LowerThresholdCritical),
          LowerThresholdFatal: GetInt(WmiCurrentProbe.LowerThresholdFatal),
          LowerThresholdNonCritical: GetInt(WmiCurrentProbe.LowerThresholdNonCritical),
          MaxReadable: GetInt(WmiCurrentProbe.MaxReadable),
          MinReadable: GetInt(WmiCurrentProbe.MinReadable),
          Name: GetStr(WmiCurrentProbe.Name),
          NominalReading: GetInt(WmiCurrentProbe.NominalReading),
          NormalMax: GetInt(WmiCurrentProbe.NormalMax),
          NormalMin: GetInt(WmiCurrentProbe.NormalMin),
          PNPDeviceID: GetStr(WmiCurrentProbe.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiCurrentProbe.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiCurrentProbe.PowerManagementSupported),
          Resolution: (uint?)GetInt(WmiCurrentProbe.Resolution),
          Status: GetStr(WmiCurrentProbe.Status),
          StatusInfo: (ushort?)GetInt(WmiCurrentProbe.StatusInfo),
          SystemCreationClassName: GetStr(WmiCurrentProbe.SystemCreationClassName),
          SystemName: GetStr(WmiCurrentProbe.SystemName),
          Tolerance: GetInt(WmiCurrentProbe.Tolerance),
          UpperThresholdCritical: GetInt(WmiCurrentProbe.UpperThresholdCritical),
          UpperThresholdFatal: GetInt(WmiCurrentProbe.UpperThresholdFatal),
          UpperThresholdNonCritical: GetInt(WmiCurrentProbe.UpperThresholdNonCritical)));
      }
      return results;
    }
    catch {
      return Array.Empty<CurrentProbeMetrics>();
    }
  }
}
