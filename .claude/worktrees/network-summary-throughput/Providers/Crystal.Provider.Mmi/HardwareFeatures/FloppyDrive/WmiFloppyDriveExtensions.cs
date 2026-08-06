using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.FloppyDrive;
public static class WmiFloppyDriveExtensions {
  private const string WmiClassName = WmiFloppyDrive.ClassName;

  public static async Task<IReadOnlyList<FloppyDriveMetrics>> ToSafeFloppyDriveMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance floppy drive data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<FloppyDriveMetrics>();
      }

      var results = new List<FloppyDriveMetrics>(instancesData.Count);

      // 2. Loop through every detected floppy drive instance sequentially
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
          ? v.AsULong() : null;
        ushort[]? GetUShortArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.UShortArray
          ? v.AsUShortArray() : null;
        string[]? GetStrArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.StringArray
          ? v.AsStringArray() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new FloppyDriveMetrics(
          Availability: (ushort?)GetInt(WmiFloppyDrive.Availability),
          Capabilities: GetUShortArr(WmiFloppyDrive.Capabilities),
          CapabilityDescriptions: GetStrArr(WmiFloppyDrive.CapabilityDescriptions),
          Caption: GetStr(WmiFloppyDrive.Caption),
          CompressionMethod: (ushort?)GetInt(WmiFloppyDrive.CompressionMethod),
          ConfigManagerErrorCode: (uint?)GetInt(WmiFloppyDrive.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiFloppyDrive.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiFloppyDrive.CreationClassName),
          DefaultBlockSize: GetULong(WmiFloppyDrive.DefaultBlockSize),
          Description: GetStr(WmiFloppyDrive.Description),
          DeviceID: GetStr(WmiFloppyDrive.DeviceID),
          ErrorCleared: GetBool(WmiFloppyDrive.ErrorCleared),
          ErrorDescription: GetStr(WmiFloppyDrive.ErrorDescription),
          ErrorMethodology: GetStr(WmiFloppyDrive.ErrorMethodology),
          InstallDate: GetDate(WmiFloppyDrive.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiFloppyDrive.LastErrorCode),
          Manufacturer: GetStr(WmiFloppyDrive.Manufacturer),
          MaxBlockSize: GetULong(WmiFloppyDrive.MaxBlockSize),
          MaxMediaSize: GetULong(WmiFloppyDrive.MaxMediaSize),
          MinBlockSize: GetULong(WmiFloppyDrive.MinBlockSize),
          Name: GetStr(WmiFloppyDrive.Name),
          NeedsCleaning: GetBool(WmiFloppyDrive.NeedsCleaning),
          NumberOfMediaSupported: (uint?)GetInt(WmiFloppyDrive.NumberOfMediaSupported),
          PNPDeviceID: GetStr(WmiFloppyDrive.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiFloppyDrive.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiFloppyDrive.PowerManagementSupported),
          Status: GetStr(WmiFloppyDrive.Status),
          StatusInfo: (ushort?)GetInt(WmiFloppyDrive.StatusInfo),
          SystemCreationClassName: GetStr(WmiFloppyDrive.SystemCreationClassName),
          SystemName: GetStr(WmiFloppyDrive.SystemName)));
      }
      return results;
    }
    catch {
      return Array.Empty<FloppyDriveMetrics>();
    }
  }
}
