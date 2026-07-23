using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.PointingDevice;
public static class WmiPointingDeviceExtensions {
  private const string WmiClassName = WmiPointingDevice.ClassName;

  public static async Task<IReadOnlyList<PointingDeviceMetrics>> ToSafePointingDeviceMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance pointing device data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<PointingDeviceMetrics>();
      }

      var results = new List<PointingDeviceMetrics>(instancesData.Count);

      // 2. Loop through every detected pointing device instance sequentially
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
        results.Add(new PointingDeviceMetrics(
          Availability: (ushort?)GetInt(WmiPointingDevice.Availability),
          Caption: GetStr(WmiPointingDevice.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiPointingDevice.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiPointingDevice.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiPointingDevice.CreationClassName),
          Description: GetStr(WmiPointingDevice.Description),
          DeviceID: GetStr(WmiPointingDevice.DeviceID),
          DeviceInterface: (ushort?)GetInt(WmiPointingDevice.DeviceInterface),
          DoubleSpeedThreshold: (uint?)GetInt(WmiPointingDevice.DoubleSpeedThreshold),
          ErrorCleared: GetBool(WmiPointingDevice.ErrorCleared),
          ErrorDescription: GetStr(WmiPointingDevice.ErrorDescription),
          Handedness: (ushort?)GetInt(WmiPointingDevice.Handedness),
          HardwareType: GetStr(WmiPointingDevice.HardwareType),
          InfFileName: GetStr(WmiPointingDevice.InfFileName),
          InfSection: GetStr(WmiPointingDevice.InfSection),
          InstallDate: GetDate(WmiPointingDevice.InstallDate),
          IsLocked: GetBool(WmiPointingDevice.IsLocked),
          LastErrorCode: (uint?)GetInt(WmiPointingDevice.LastErrorCode),
          Manufacturer: GetStr(WmiPointingDevice.Manufacturer),
          Name: GetStr(WmiPointingDevice.Name),
          NumberOfButtons: (byte?)GetInt(WmiPointingDevice.NumberOfButtons),
          PNPDeviceID: GetStr(WmiPointingDevice.PNPDeviceID),
          PointingType: (ushort?)GetInt(WmiPointingDevice.PointingType),
          PowerManagementCapabilities: GetUShortArr(WmiPointingDevice.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiPointingDevice.PowerManagementSupported),
          QuadSpeedThreshold: (uint?)GetInt(WmiPointingDevice.QuadSpeedThreshold),
          Resolution: (uint?)GetInt(WmiPointingDevice.Resolution),
          SampleRate: (uint?)GetInt(WmiPointingDevice.SampleRate),
          Status: GetStr(WmiPointingDevice.Status),
          StatusInfo: (ushort?)GetInt(WmiPointingDevice.StatusInfo),
          Synch: (uint?)GetInt(WmiPointingDevice.Synch),
          SystemCreationClassName: GetStr(WmiPointingDevice.SystemCreationClassName),
          SystemName: GetStr(WmiPointingDevice.SystemName)));
      }
      return results;
    }
    catch {
      return Array.Empty<PointingDeviceMetrics>();
    }
  }
}
