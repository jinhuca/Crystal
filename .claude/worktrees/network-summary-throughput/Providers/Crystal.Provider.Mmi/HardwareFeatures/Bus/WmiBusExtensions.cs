using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.Bus;
public static class WmiBusExtensions {
  private const string WmiClassName = WmiBus.ClassName;

  public static async Task<IReadOnlyList<BusMetrics>> ToSafeBusMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance bus data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<BusMetrics>();
      }

      var results = new List<BusMetrics>(instancesData.Count);

      // 2. Loop through every detected bus instance sequentially
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
        results.Add(new BusMetrics(
          Availability: (ushort?)GetInt(WmiBus.Availability),
          BusNum: (uint?)GetInt(WmiBus.BusNum),
          BusType: (uint?)GetInt(WmiBus.BusType),
          Caption: GetStr(WmiBus.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiBus.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiBus.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiBus.CreationClassName),
          Description: GetStr(WmiBus.Description),
          DeviceID: GetStr(WmiBus.DeviceID),
          ErrorCleared: GetBool(WmiBus.ErrorCleared),
          ErrorDescription: GetStr(WmiBus.ErrorDescription),
          InstallDate: GetDate(WmiBus.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiBus.LastErrorCode),
          Name: GetStr(WmiBus.Name),
          PNPDeviceID: GetStr(WmiBus.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiBus.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiBus.PowerManagementSupported),
          Status: GetStr(WmiBus.Status),
          StatusInfo: (ushort?)GetInt(WmiBus.StatusInfo),
          SystemCreationClassName: GetStr(WmiBus.SystemCreationClassName),
          SystemName: GetStr(WmiBus.SystemName)));
      }
      return results;
    }
    catch {
      return Array.Empty<BusMetrics>();
    }
  }
}
