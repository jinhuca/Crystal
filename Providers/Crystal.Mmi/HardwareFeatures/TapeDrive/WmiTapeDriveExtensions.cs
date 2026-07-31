using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.TapeDrive;

public static class WmiTapeDriveExtensions {
  private const string WmiClassName = WmiTapeDrive.ClassName;

  public static async Task<IReadOnlyList<TapeDriveMetrics>> ToSafeTapeDriveMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance tape drive data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<TapeDriveMetrics>();
      }

      var results = new List<TapeDriveMetrics>(instancesData.Count);

      // 2. Loop through every single detected tape drive device
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
          ? v.AsReadOnlyULong() : null;
        ushort[]? GetUShortArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.UShortArray
          ? v.AsUShortArray() : null;
        string[]? GetStrArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.StringArray
          ? v.AsStringArray() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new TapeDriveMetrics(
          Availability: (ushort?)GetInt(WmiTapeDrive.Availability),
          Capabilities: GetUShortArr(WmiTapeDrive.Capabilities),
          CapabilityDescriptions: GetStrArr(WmiTapeDrive.CapabilityDescriptions),
          Caption: GetStr(WmiTapeDrive.Caption),
          Compression: (uint?)GetInt(WmiTapeDrive.Compression),
          CompressionMethod: GetStr(WmiTapeDrive.CompressionMethod),
          ConfigManagerErrorCode: (uint?)GetInt(WmiTapeDrive.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiTapeDrive.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiTapeDrive.CreationClassName),
          DefaultBlockSize: GetULong(WmiTapeDrive.DefaultBlockSize),
          Description: GetStr(WmiTapeDrive.Description),
          DeviceID: GetStr(WmiTapeDrive.DeviceID),
          ECC: GetStr(WmiTapeDrive.ECC),
          ErrorCleared: GetBool(WmiTapeDrive.ErrorCleared),
          ErrorDescription: GetStr(WmiTapeDrive.ErrorDescription),
          ErrorMethodology: GetStr(WmiTapeDrive.ErrorMethodology),
          FeaturesHigh: (uint?)GetInt(WmiTapeDrive.FeaturesHigh),
          FeaturesLow: (uint?)GetInt(WmiTapeDrive.FeaturesLow),
          Id: GetStr(WmiTapeDrive.Id),
          InstallDate: GetDate(WmiTapeDrive.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiTapeDrive.LastErrorCode),
          Manufacturer: GetStr(WmiTapeDrive.Manufacturer),
          MaxBlockSize: GetULong(WmiTapeDrive.MaxBlockSize),
          MaxMediaSize: (uint?)GetInt(WmiTapeDrive.MaxMediaSize),
          MaxPartitionCount: (uint?)GetInt(WmiTapeDrive.MaxPartitionCount),
          MediaType: GetStr(WmiTapeDrive.MediaType),
          MinBlockSize: GetULong(WmiTapeDrive.MinBlockSize),
          Name: GetStr(WmiTapeDrive.Name),
          NeedsCleaning: GetBool(WmiTapeDrive.NeedsCleaning),
          NumberOfMediaSupported: (uint?)GetInt(WmiTapeDrive.NumberOfMediaSupported),
          Padding: (uint?)GetInt(WmiTapeDrive.Padding),
          PNPDeviceID: GetStr(WmiTapeDrive.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiTapeDrive.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiTapeDrive.PowerManagementSupported),
          ReportSetMarks: GetStr(WmiTapeDrive.ReportSetMarks),
          Status: GetStr(WmiTapeDrive.Status),
          StatusInfo: (ushort?)GetInt(WmiTapeDrive.StatusInfo),
          SystemCreationClassName: GetStr(WmiTapeDrive.SystemCreationClassName),
          SystemName: GetStr(WmiTapeDrive.SystemName)));
      }
      return results;
    }
    catch {
      return Array.Empty<TapeDriveMetrics>();
    }
  }
}
