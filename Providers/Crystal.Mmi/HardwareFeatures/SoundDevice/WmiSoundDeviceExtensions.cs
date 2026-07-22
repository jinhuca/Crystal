using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.SoundDevice;

public static class WmiSoundDeviceExtensions {
  private const string WmiClassName = WmiSoundDevice.ClassName;

  public static async Task<IReadOnlyList<SoundDeviceMetrics>> ToSafeSoundDeviceMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance sound card data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if(instancesData == null || instancesData.Count == 0) {
        return Array.Empty<SoundDeviceMetrics>();
      }

      var results = new List<SoundDeviceMetrics>(instancesData.Count);

      // 2. Loop through every detected physical audio driver instance sequentially
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

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new SoundDeviceMetrics(
          Availability: (ushort?)GetInt(WmiSoundDevice.Availability),
          Caption: GetStr(WmiSoundDevice.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiSoundDevice.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiSoundDevice.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiSoundDevice.CreationClassName),
          Description: GetStr(WmiSoundDevice.Description),
          DeviceID: GetStr(WmiSoundDevice.DeviceID),
          DMABufferSize: (ushort?)GetInt(WmiSoundDevice.DMABufferSize),
          ErrorCleared: GetBool(WmiSoundDevice.ErrorCleared),
          ErrorDescription: GetStr(WmiSoundDevice.ErrorDescription),
          InstallDate: GetDate(WmiSoundDevice.InstallationDate),
          LastErrorCode: (uint?)GetInt(WmiSoundDevice.LastErrorCode),
          Manufacturer: GetStr(WmiSoundDevice.Manufacturer),
          MPU401Address: GetStr(WmiSoundDevice.MPU401Address),
          Name: GetStr(WmiSoundDevice.Name),
          PNPDeviceID: GetStr(WmiSoundDevice.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiSoundDevice.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiSoundDevice.PowerManagementSupported),
          ProductID: GetStr(WmiSoundDevice.ProductID),
          Status: GetStr(WmiSoundDevice.Status),
          StatusInfo: (ushort?)GetInt(WmiSoundDevice.StatusInfo),
          SystemCreationClassName: GetStr(WmiSoundDevice.SystemCreationClassName),
          SystemName: GetStr(WmiSoundDevice.SystemName)));
      }
      return results;
    }
    catch {
      return Array.Empty<SoundDeviceMetrics>();
    }
  }
}
