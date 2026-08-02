using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.Fan;
public static class WmiFanExtensions {
  private const string WmiClassName = WmiFan.ClassName;

  public static async Task<IReadOnlyList<FanMetrics>> ToSafeFanMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance fan data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<FanMetrics>();
      }

      var results = new List<FanMetrics>(instancesData.Count);

      // 2. Loop through every detected fan instance sequentially
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
        ulong? GetULong(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.ULong 
          ? v.AsReadOnlyULong() : null;
        ushort[]? GetUShortArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.UShortArray 
          ? v.AsUShortArray() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new FanMetrics(
          ActiveCooling: GetBool(WmiFan.ActiveCooling),
          Availability: (ushort?)GetInt(WmiFan.Availability),
          Caption: GetStr(WmiFan.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiFan.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiFan.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiFan.CreationClassName),
          Description: GetStr(WmiFan.Description),
          DesiredSpeed: GetULong(WmiFan.DesiredSpeed),
          DeviceID: GetStr(WmiFan.DeviceID),
          ErrorCleared: GetBool(WmiFan.ErrorCleared),
          ErrorDescription: GetStr(WmiFan.ErrorDescription),
          InstallDate: GetDate(WmiFan.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiFan.LastErrorCode),
          Name: GetStr(WmiFan.Name),
          PNPDeviceID: GetStr(WmiFan.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiFan.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiFan.PowerManagementSupported),
          Status: GetStr(WmiFan.Status),
          StatusInfo: (ushort?)GetInt(WmiFan.StatusInfo),
          SystemCreationClassName: GetStr(WmiFan.SystemCreationClassName),
          SystemName: GetStr(WmiFan.SystemName),
          VariableSpeed: GetBool(WmiFan.VariableSpeed)));
      }
      return results;
    }
    catch {
      return Array.Empty<FanMetrics>();
    }
  }
}
