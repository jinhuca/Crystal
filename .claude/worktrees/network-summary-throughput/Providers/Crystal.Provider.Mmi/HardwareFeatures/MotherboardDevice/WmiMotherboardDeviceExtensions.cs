using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.MotherboardDevice;
public static class WmiMotherboardDeviceExtensions {
  private const string WmiClassName = WmiMotherboardDevice.ClassName;

  public static async Task<IReadOnlyList<MotherboardDeviceMetrics>> ToSafeMotherboardDeviceMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance motherboard device data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<MotherboardDeviceMetrics>();
      }

      var results = new List<MotherboardDeviceMetrics>(instancesData.Count);

      // 2. Loop through every detected motherboard device instance sequentially
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
        results.Add(new MotherboardDeviceMetrics(
          Availability: (ushort?)GetInt(WmiMotherboardDevice.Availability),
          Caption: GetStr(WmiMotherboardDevice.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiMotherboardDevice.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiMotherboardDevice.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiMotherboardDevice.CreationClassName),
          Description: GetStr(WmiMotherboardDevice.Description),
          DeviceID: GetStr(WmiMotherboardDevice.DeviceID),
          ErrorCleared: GetBool(WmiMotherboardDevice.ErrorCleared),
          ErrorDescription: GetStr(WmiMotherboardDevice.ErrorDescription),
          InstallDate: GetDate(WmiMotherboardDevice.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiMotherboardDevice.LastErrorCode),
          Name: GetStr(WmiMotherboardDevice.Name),
          PNPDeviceID: GetStr(WmiMotherboardDevice.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiMotherboardDevice.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiMotherboardDevice.PowerManagementSupported),
          PrimaryBusType: GetStr(WmiMotherboardDevice.PrimaryBusType),
          RevisionNumber: GetStr(WmiMotherboardDevice.RevisionNumber),
          SecondaryBusType: GetStr(WmiMotherboardDevice.SecondaryBusType),
          Status: GetStr(WmiMotherboardDevice.Status),
          StatusInfo: (ushort?)GetInt(WmiMotherboardDevice.StatusInfo),
          SystemCreationClassName: GetStr(WmiMotherboardDevice.SystemCreationClassName),
          SystemName: GetStr(WmiMotherboardDevice.SystemName)));
      }
      return results;
    }
    catch {
      return Array.Empty<MotherboardDeviceMetrics>();
    }
  }
}
