using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.HeatPipe;
public static class WmiHeatPipeExtensions {
  private const string WmiClassName = WmiHeatPipe.ClassName;

  public static async Task<IReadOnlyList<HeatPipeMetrics>> ToSafeHeatPipeMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance heat pipe data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<HeatPipeMetrics>();
      }

      var results = new List<HeatPipeMetrics>(instancesData.Count);

      // 2. Loop through every detected heat pipe instance sequentially
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
        results.Add(new HeatPipeMetrics(
          ActiveCooling: GetBool(WmiHeatPipe.ActiveCooling),
          Availability: (ushort?)GetInt(WmiHeatPipe.Availability),
          Caption: GetStr(WmiHeatPipe.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiHeatPipe.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiHeatPipe.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiHeatPipe.CreationClassName),
          Description: GetStr(WmiHeatPipe.Description),
          DeviceID: GetStr(WmiHeatPipe.DeviceID),
          ErrorCleared: GetBool(WmiHeatPipe.ErrorCleared),
          ErrorDescription: GetStr(WmiHeatPipe.ErrorDescription),
          InstallDate: GetDate(WmiHeatPipe.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiHeatPipe.LastErrorCode),
          Name: GetStr(WmiHeatPipe.Name),
          PNPDeviceID: GetStr(WmiHeatPipe.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiHeatPipe.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiHeatPipe.PowerManagementSupported),
          Status: GetStr(WmiHeatPipe.Status),
          StatusInfo: (ushort?)GetInt(WmiHeatPipe.StatusInfo),
          SystemCreationClassName: GetStr(WmiHeatPipe.SystemCreationClassName),
          SystemName: GetStr(WmiHeatPipe.SystemName)));
      }
      return results;
    }
    catch {
      return Array.Empty<HeatPipeMetrics>();
    }
  }
}
