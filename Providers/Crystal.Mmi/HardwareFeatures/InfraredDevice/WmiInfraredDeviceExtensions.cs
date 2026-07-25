using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.InfraredDevice;
public static class WmiInfraredDeviceExtensions {
  private const string WmiClassName = WmiInfraredDevice.ClassName;

  public static async Task<IReadOnlyList<InfraredDeviceMetrics>> ToSafeInfraredDeviceMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance infrared device data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<InfraredDeviceMetrics>();
      }

      var results = new List<InfraredDeviceMetrics>(instancesData.Count);

      // 2. Loop through every detected infrared device instance sequentially
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
        results.Add(new InfraredDeviceMetrics(
          Availability: (ushort?)GetInt(WmiInfraredDevice.Availability),
          Caption: GetStr(WmiInfraredDevice.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiInfraredDevice.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiInfraredDevice.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiInfraredDevice.CreationClassName),
          Description: GetStr(WmiInfraredDevice.Description),
          DeviceID: GetStr(WmiInfraredDevice.DeviceID),
          ErrorCleared: GetBool(WmiInfraredDevice.ErrorCleared),
          ErrorDescription: GetStr(WmiInfraredDevice.ErrorDescription),
          InstallDate: GetDate(WmiInfraredDevice.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiInfraredDevice.LastErrorCode),
          Manufacturer: GetStr(WmiInfraredDevice.Manufacturer),
          MaxNumberControlled: (uint?)GetInt(WmiInfraredDevice.MaxNumberControlled),
          Name: GetStr(WmiInfraredDevice.Name),
          PNPDeviceID: GetStr(WmiInfraredDevice.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiInfraredDevice.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiInfraredDevice.PowerManagementSupported),
          ProtocolSupported: (ushort?)GetInt(WmiInfraredDevice.ProtocolSupported),
          Status: GetStr(WmiInfraredDevice.Status),
          StatusInfo: (ushort?)GetInt(WmiInfraredDevice.StatusInfo),
          SystemCreationClassName: GetStr(WmiInfraredDevice.SystemCreationClassName),
          SystemName: GetStr(WmiInfraredDevice.SystemName),
          TimeOfLastReset: GetDate(WmiInfraredDevice.TimeOfLastReset)));
      }
      return results;
    }
    catch {
      return Array.Empty<InfraredDeviceMetrics>();
    }
  }
}
