using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.USBHub;
public static class WmiUSBHubExtensions {
  private const string WmiClassName = WmiUSBHub.ClassName;

  public static async Task<IReadOnlyList<USBHubMetrics>> ToSafeUSBHubMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance USB hub data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<USBHubMetrics>();
      }

      var results = new List<USBHubMetrics>(instancesData.Count);

      // 2. Loop through every detected USB hub instance sequentially
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
        // USB descriptor byte arrays (uint8[] in real WMI) come back through the same
        // UShortArray channel used elsewhere, since every value fits in 0-255.
        byte[]? GetByteArr(string key) => GetUShortArr(key)?.Select(x => (byte)x).ToArray();

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new USBHubMetrics(
          Availability: (ushort?)GetInt(WmiUSBHub.Availability),
          Caption: GetStr(WmiUSBHub.Caption),
          ClassCode: (byte?)GetInt(WmiUSBHub.ClassCode),
          ConfigManagerErrorCode: (uint?)GetInt(WmiUSBHub.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiUSBHub.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiUSBHub.CreationClassName),
          CurrentAlternateSettings: GetByteArr(WmiUSBHub.CurrentAlternateSettings),
          CurrentConfigValue: (byte?)GetInt(WmiUSBHub.CurrentConfigValue),
          Description: GetStr(WmiUSBHub.Description),
          DeviceID: GetStr(WmiUSBHub.DeviceID),
          ErrorCleared: GetBool(WmiUSBHub.ErrorCleared),
          ErrorDescription: GetStr(WmiUSBHub.ErrorDescription),
          GangSwitched: GetBool(WmiUSBHub.GangSwitched),
          InstallDate: GetDate(WmiUSBHub.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiUSBHub.LastErrorCode),
          Name: GetStr(WmiUSBHub.Name),
          NumberOfConfigs: (byte?)GetInt(WmiUSBHub.NumberOfConfigs),
          NumberOfPorts: (byte?)GetInt(WmiUSBHub.NumberOfPorts),
          PNPDeviceID: GetStr(WmiUSBHub.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiUSBHub.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiUSBHub.PowerManagementSupported),
          ProtocolCode: (byte?)GetInt(WmiUSBHub.ProtocolCode),
          Status: GetStr(WmiUSBHub.Status),
          StatusInfo: (ushort?)GetInt(WmiUSBHub.StatusInfo),
          SubclassCode: (byte?)GetInt(WmiUSBHub.SubclassCode),
          SystemCreationClassName: GetStr(WmiUSBHub.SystemCreationClassName),
          SystemName: GetStr(WmiUSBHub.SystemName),
          USBVersion: (ushort?)GetInt(WmiUSBHub.USBVersion)));
      }
      return results;
    }
    catch {
      return Array.Empty<USBHubMetrics>();
    }
  }
}
