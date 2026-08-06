using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.PnPEntity;

public static class WmiPnPEntityExtensions {
  private const string WmiClassName = WmiPnPEntity.ClassName;

  public static async Task<IReadOnlyList<PnPEntityMetrics>> ToSafePnPEntityMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance PnP data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if(instancesData == null || instancesData.Count == 0) {
        return Array.Empty<PnPEntityMetrics>();
      }

      var results = new List<PnPEntityMetrics>(instancesData.Count);

      // 2. Loop through every detected PnP device sequentially
      foreach(var data in instancesData) {
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
        string? FlattenStrArray(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.StringArray 
          ? string.Join(", ", v.AsStringArray() ?? Array.Empty<string>()) : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new PnPEntityMetrics(
          Availability: (ushort?)GetInt(WmiPnPEntity.Availability),
          Caption: GetStr(WmiPnPEntity.Caption),
          ClassGuid: GetStr(WmiPnPEntity.ClassGuid),
          CompatibleID: FlattenStrArray(WmiPnPEntity.CompatibleID),
          ConfigManagerErrorCode: (uint?)GetInt(WmiPnPEntity.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiPnPEntity.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiPnPEntity.CreationClassName),
          Description: GetStr(WmiPnPEntity.Description),
          DeviceID: GetStr(WmiPnPEntity.DeviceID),
          ErrorCleared: GetBool(WmiPnPEntity.ErrorCleared),
          ErrorDescription: GetStr(WmiPnPEntity.ErrorDescription),
          HardwareID: FlattenStrArray(WmiPnPEntity.HardwareID),
          InstallDate: GetDate(WmiPnPEntity.InstallationDate),
          LastErrorCode: (uint?)GetInt(WmiPnPEntity.LastErrorCode),
          Manufacturer: GetStr(WmiPnPEntity.Manufacturer),
          Name: GetStr(WmiPnPEntity.Name),
          PNPClass: GetStr(WmiPnPEntity.PNPClass),
          PNPDeviceID: GetStr(WmiPnPEntity.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiPnPEntity.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiPnPEntity.PowerManagementSupported),
          Present: GetBool(WmiPnPEntity.Present),
          Service: GetStr(WmiPnPEntity.Service),
          Status: GetStr(WmiPnPEntity.Status),
          StatusInfo: (ushort?)GetInt(WmiPnPEntity.StatusInfo),
          SystemCreationClassName: GetStr(WmiPnPEntity.SystemCreationClassName),
          SystemName: GetStr(WmiPnPEntity.SystemName)));
      }
      return results;
    }
    catch {
      return Array.Empty<PnPEntityMetrics>();
    }
  }
}
