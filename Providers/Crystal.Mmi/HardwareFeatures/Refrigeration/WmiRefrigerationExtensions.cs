using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.Refrigeration;
public static class WmiRefrigerationExtensions {
  private const string WmiClassName = WmiRefrigeration.ClassName;

  public static async Task<IReadOnlyList<RefrigerationMetrics>> ToSafeRefrigerationMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance refrigeration device data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<RefrigerationMetrics>();
      }

      var results = new List<RefrigerationMetrics>(instancesData.Count);

      // 2. Loop through every detected refrigeration device instance sequentially
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
        results.Add(new RefrigerationMetrics(
          ActiveCooling: GetBool(WmiRefrigeration.ActiveCooling),
          Availability: (ushort?)GetInt(WmiRefrigeration.Availability),
          Caption: GetStr(WmiRefrigeration.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiRefrigeration.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiRefrigeration.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiRefrigeration.CreationClassName),
          Description: GetStr(WmiRefrigeration.Description),
          DeviceID: GetStr(WmiRefrigeration.DeviceID),
          ErrorCleared: GetBool(WmiRefrigeration.ErrorCleared),
          ErrorDescription: GetStr(WmiRefrigeration.ErrorDescription),
          InstallDate: GetDate(WmiRefrigeration.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiRefrigeration.LastErrorCode),
          Name: GetStr(WmiRefrigeration.Name),
          PNPDeviceID: GetStr(WmiRefrigeration.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiRefrigeration.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiRefrigeration.PowerManagementSupported),
          Status: GetStr(WmiRefrigeration.Status),
          StatusInfo: (ushort?)GetInt(WmiRefrigeration.StatusInfo),
          SystemCreationClassName: GetStr(WmiRefrigeration.SystemCreationClassName),
          SystemName: GetStr(WmiRefrigeration.SystemName)));
      }
      return results;
    }
    catch {
      return Array.Empty<RefrigerationMetrics>();
    }
  }
}
