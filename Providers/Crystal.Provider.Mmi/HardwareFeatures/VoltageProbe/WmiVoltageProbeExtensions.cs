using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.VoltageProbe;
public static class WmiVoltageProbeExtensions {
  private const string WmiClassName = WmiVoltageProbe.ClassName;

  public static async Task<IReadOnlyList<VoltageProbeMetrics>> ToSafeVoltageProbeMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance voltage probe data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<VoltageProbeMetrics>();
      }

      var results = new List<VoltageProbeMetrics>(instancesData.Count);

      // 2. Loop through every detected voltage probe instance sequentially
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
        results.Add(new VoltageProbeMetrics(
          Accuracy: GetInt(WmiVoltageProbe.Accuracy),
          Availability: (ushort?)GetInt(WmiVoltageProbe.Availability),
          Caption: GetStr(WmiVoltageProbe.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiVoltageProbe.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiVoltageProbe.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiVoltageProbe.CreationClassName),
          CurrentReading: GetInt(WmiVoltageProbe.CurrentReading),
          Description: GetStr(WmiVoltageProbe.Description),
          DeviceID: GetStr(WmiVoltageProbe.DeviceID),
          ErrorCleared: GetBool(WmiVoltageProbe.ErrorCleared),
          ErrorDescription: GetStr(WmiVoltageProbe.ErrorDescription),
          InstallDate: GetDate(WmiVoltageProbe.InstallDate),
          IsLinear: GetBool(WmiVoltageProbe.IsLinear),
          LastErrorCode: (uint?)GetInt(WmiVoltageProbe.LastErrorCode),
          LowerThresholdCritical: GetInt(WmiVoltageProbe.LowerThresholdCritical),
          LowerThresholdFatal: GetInt(WmiVoltageProbe.LowerThresholdFatal),
          LowerThresholdNonCritical: GetInt(WmiVoltageProbe.LowerThresholdNonCritical),
          MaxReadable: GetInt(WmiVoltageProbe.MaxReadable),
          MinReadable: GetInt(WmiVoltageProbe.MinReadable),
          Name: GetStr(WmiVoltageProbe.Name),
          NominalReading: GetInt(WmiVoltageProbe.NominalReading),
          NormalMax: GetInt(WmiVoltageProbe.NormalMax),
          NormalMin: GetInt(WmiVoltageProbe.NormalMin),
          PNPDeviceID: GetStr(WmiVoltageProbe.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiVoltageProbe.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiVoltageProbe.PowerManagementSupported),
          Resolution: (uint?)GetInt(WmiVoltageProbe.Resolution),
          Status: GetStr(WmiVoltageProbe.Status),
          StatusInfo: (ushort?)GetInt(WmiVoltageProbe.StatusInfo),
          SystemCreationClassName: GetStr(WmiVoltageProbe.SystemCreationClassName),
          SystemName: GetStr(WmiVoltageProbe.SystemName),
          Tolerance: GetInt(WmiVoltageProbe.Tolerance),
          UpperThresholdCritical: GetInt(WmiVoltageProbe.UpperThresholdCritical),
          UpperThresholdFatal: GetInt(WmiVoltageProbe.UpperThresholdFatal),
          UpperThresholdNonCritical: GetInt(WmiVoltageProbe.UpperThresholdNonCritical)));
      }
      return results;
    }
    catch {
      return Array.Empty<VoltageProbeMetrics>();
    }
  }
}
