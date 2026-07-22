using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.LogicalDisk;
public static class WmiLogicalDiskExtensions {
  private const string WmiClassName = WmiLogicalDisk.ClassName;

  public static async Task<IReadOnlyList<LogicalDiskMetrics>> ToSafeLogicalDiskMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance partition volumes asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<LogicalDiskMetrics>();
      }

      var results = new List<LogicalDiskMetrics>(instancesData.Count);

      // 2. Loop through every detected mounted storage volume location
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
        ulong? GetULong(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.ULong 
          ? v.AsReadOnlyULong() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new LogicalDiskMetrics(
          Availability: (ushort?)GetInt(WmiLogicalDisk.Availability),
          BlockSize: GetULong(WmiLogicalDisk.BlockSize),
          Caption: GetStr(WmiLogicalDisk.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiLogicalDisk.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiLogicalDisk.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiLogicalDisk.CreationClassName),
          Description: GetStr(WmiLogicalDisk.Description),
          DeviceID: GetStr(WmiLogicalDisk.DeviceID),
          DriveType: (uint?)GetInt(WmiLogicalDisk.DriveType),
          ErrorCleared: GetBool(WmiLogicalDisk.ErrorCleared),
          ErrorDescription: GetStr(WmiLogicalDisk.ErrorDescription),
          ErrorMethodology: GetStr(WmiLogicalDisk.ErrorMethodology),
          FileSystem: GetStr(WmiLogicalDisk.FileSystem),
          FreeSpace: GetULong(WmiLogicalDisk.FreeSpace),
          InstallDate: GetDate(WmiLogicalDisk.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiLogicalDisk.LastErrorCode),
          MaximumComponentLength: (uint?)GetInt(WmiLogicalDisk.MaximumComponentLength),
          Name: GetStr(WmiLogicalDisk.Name),
          NumberOfBlocks: GetULong(WmiLogicalDisk.NumberOfBlocks),
          PNPDeviceID: GetStr(WmiLogicalDisk.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiLogicalDisk.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiLogicalDisk.PowerManagementSupported),
          ProviderName: GetStr(WmiLogicalDisk.ProviderName),
          Purpose: null,
          Size: GetULong(WmiLogicalDisk.Size),
          Status: GetStr(WmiLogicalDisk.Status),
          StatusInfo: (ushort?)GetInt(WmiLogicalDisk.StatusInfo),
          SupportsDiskQuotas: GetBool(WmiLogicalDisk.SupportsDiskQuotas),
          SupportsFileBasedCompression: GetBool(WmiLogicalDisk.SupportsFileBasedCompression),
          SystemCreationClassName: GetStr(WmiLogicalDisk.SystemCreationClassName),
          SystemName: GetStr(WmiLogicalDisk.SystemName),
          VolumeName: GetStr(WmiLogicalDisk.VolumeName),
          VolumeSerialNumber: GetStr(WmiLogicalDisk.VolumeSerialNumber)));
      }

      return results;
    }
    catch {
      return Array.Empty<LogicalDiskMetrics>();
    }
  }
}
