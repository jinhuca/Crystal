using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.SystemEnclosure;
public static class WmiSystemEnclosureExtensions {
  private const string WmiClassName = WmiSystemEnclosure.ClassName;

  public static async Task<IReadOnlyList<SystemEnclosureMetrics>> ToSafeSystemEnclosureMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance casing data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<SystemEnclosureMetrics>();
      }

      var results = new List<SystemEnclosureMetrics>(instancesData.Count);

      // 2. Loop through every detected structural enclosure instance sequentially
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
        results.Add(new SystemEnclosureMetrics(
          AssetTag: GetStr(WmiSystemEnclosure.AssetTag),
          AudibleAlarm: GetBool(WmiSystemEnclosure.AudibleAlarm),
          BreachDescription: GetStr(WmiSystemEnclosure.BreachDescription),
          CableManagementStrategy: (ushort?)GetInt(WmiSystemEnclosure.CableManagementStrategy),
          Caption: GetStr(WmiSystemEnclosure.Caption),
          ChassisTypes: GetUShortArr(WmiSystemEnclosure.ChassisTypes),
          CreationClassName: GetStr(WmiSystemEnclosure.CreationClassName),
          Description: GetStr(WmiSystemEnclosure.Description),
          HeatSinkPresent: GetBool(WmiSystemEnclosure.HeatSinkPresent),
          HotSwappable: GetBool(WmiSystemEnclosure.HotSwappable),
          InstallDate: GetDate(WmiSystemEnclosure.InstallationDate),
          LastErrorCode: GetStr(WmiSystemEnclosure.LastErrorCode),
          LockPresent: GetBool(WmiSystemEnclosure.LockPresent),
          SecurityStatus: (ushort?)GetInt(WmiSystemEnclosure.SecurityStatus),
          SerialNumber: GetStr(WmiSystemEnclosure.SerialNumber),
          SMBIOSAssetTag: GetStr(WmiSystemEnclosure.SMBIOSAssetTag),
          Status: GetStr(WmiSystemEnclosure.Status),
          StatusInfo: (ushort?)GetInt(WmiSystemEnclosure.StatusInfo),
          SystemCreationClassName: GetStr(WmiSystemEnclosure.SystemCreationClassName),
          SystemName: GetStr(WmiSystemEnclosure.SystemName),
          Tag: GetStr(WmiSystemEnclosure.Tag),
          SecurityBreach: (ushort?)GetInt(WmiSystemEnclosure.SecurityBreach),
          Version: GetStr(WmiSystemEnclosure.Version),
          VisibleAlarm: GetBool(WmiSystemEnclosure.VisibleAlarm)));
      }
      return results;
    }
    catch {
      return Array.Empty<SystemEnclosureMetrics>();
    }
  }
}
