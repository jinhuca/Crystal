using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.TemperatureProbe;
public static class WmiTemperatureProbeExtensions {
  private const string WmiClassName = WmiTemperatureProbe.ClassName;

  public static async Task<IReadOnlyList<TemperatureProbeMetrics>> ToSafeTemperatureProbeMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance temperature probe data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<TemperatureProbeMetrics>();
      }

      var results = new List<TemperatureProbeMetrics>(instancesData.Count);

      // 2. Loop through every detected temperature probe instance sequentially
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
        results.Add(new TemperatureProbeMetrics(
          Accuracy: GetInt(WmiTemperatureProbe.Accuracy),
          Availability: (ushort?)GetInt(WmiTemperatureProbe.Availability),
          Caption: GetStr(WmiTemperatureProbe.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiTemperatureProbe.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiTemperatureProbe.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiTemperatureProbe.CreationClassName),
          CurrentReading: GetInt(WmiTemperatureProbe.CurrentReading),
          Description: GetStr(WmiTemperatureProbe.Description),
          DeviceID: GetStr(WmiTemperatureProbe.DeviceID),
          ErrorCleared: GetBool(WmiTemperatureProbe.ErrorCleared),
          ErrorDescription: GetStr(WmiTemperatureProbe.ErrorDescription),
          InstallDate: GetDate(WmiTemperatureProbe.InstallDate),
          IsLinear: GetBool(WmiTemperatureProbe.IsLinear),
          LastErrorCode: (uint?)GetInt(WmiTemperatureProbe.LastErrorCode),
          LowerThresholdCritical: GetInt(WmiTemperatureProbe.LowerThresholdCritical),
          LowerThresholdFatal: GetInt(WmiTemperatureProbe.LowerThresholdFatal),
          LowerThresholdNonCritical: GetInt(WmiTemperatureProbe.LowerThresholdNonCritical),
          MaxReadable: GetInt(WmiTemperatureProbe.MaxReadable),
          MinReadable: GetInt(WmiTemperatureProbe.MinReadable),
          Name: GetStr(WmiTemperatureProbe.Name),
          NominalReading: GetInt(WmiTemperatureProbe.NominalReading),
          NormalMax: GetInt(WmiTemperatureProbe.NormalMax),
          NormalMin: GetInt(WmiTemperatureProbe.NormalMin),
          PNPDeviceID: GetStr(WmiTemperatureProbe.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiTemperatureProbe.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiTemperatureProbe.PowerManagementSupported),
          Resolution: (uint?)GetInt(WmiTemperatureProbe.Resolution),
          Status: GetStr(WmiTemperatureProbe.Status),
          StatusInfo: (ushort?)GetInt(WmiTemperatureProbe.StatusInfo),
          SystemCreationClassName: GetStr(WmiTemperatureProbe.SystemCreationClassName),
          SystemName: GetStr(WmiTemperatureProbe.SystemName),
          Tolerance: GetInt(WmiTemperatureProbe.Tolerance),
          UpperThresholdCritical: GetInt(WmiTemperatureProbe.UpperThresholdCritical),
          UpperThresholdFatal: GetInt(WmiTemperatureProbe.UpperThresholdFatal),
          UpperThresholdNonCritical: GetInt(WmiTemperatureProbe.UpperThresholdNonCritical)));
      }
      return results;
    }
    catch {
      return Array.Empty<TemperatureProbeMetrics>();
    }
  }
}
