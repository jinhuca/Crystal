using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.MappedLogicalDisk;
public static class WmiMappedLogicalDiskExtensions {
  private const string WmiClassName = WmiMappedLogicalDisk.ClassName;

  public static async Task<IReadOnlyList<MappedLogicalDiskMetrics>> ToSafeMappedLogicalDiskMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance mapped-drive data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<MappedLogicalDiskMetrics>();
      }

      var results = new List<MappedLogicalDiskMetrics>(instancesData.Count);

      // 2. Loop through every detected mapped-drive instance sequentially
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

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new MappedLogicalDiskMetrics(
          Access: (ushort?)GetInt(WmiMappedLogicalDisk.Access),
          Availability: (ushort?)GetInt(WmiMappedLogicalDisk.Availability),
          BlockSize: GetULong(WmiMappedLogicalDisk.BlockSize),
          Caption: GetStr(WmiMappedLogicalDisk.Caption),
          Compressed: GetBool(WmiMappedLogicalDisk.Compressed),
          ConfigManagerErrorCode: (uint?)GetInt(WmiMappedLogicalDisk.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiMappedLogicalDisk.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiMappedLogicalDisk.CreationClassName),
          Description: GetStr(WmiMappedLogicalDisk.Description),
          DeviceID: GetStr(WmiMappedLogicalDisk.DeviceID),
          ErrorCleared: GetBool(WmiMappedLogicalDisk.ErrorCleared),
          ErrorDescription: GetStr(WmiMappedLogicalDisk.ErrorDescription),
          ErrorMethodology: GetStr(WmiMappedLogicalDisk.ErrorMethodology),
          FileSystem: GetStr(WmiMappedLogicalDisk.FileSystem),
          FreeSpace: GetULong(WmiMappedLogicalDisk.FreeSpace),
          InstallDate: GetDate(WmiMappedLogicalDisk.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiMappedLogicalDisk.LastErrorCode),
          MaximumComponentLength: (uint?)GetInt(WmiMappedLogicalDisk.MaximumComponentLength),
          Name: GetStr(WmiMappedLogicalDisk.Name),
          NumberOfBlocks: GetULong(WmiMappedLogicalDisk.NumberOfBlocks),
          PNPDeviceID: GetStr(WmiMappedLogicalDisk.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiMappedLogicalDisk.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiMappedLogicalDisk.PowerManagementSupported),
          ProviderName: GetStr(WmiMappedLogicalDisk.ProviderName),
          Purpose: GetStr(WmiMappedLogicalDisk.Purpose),
          QuotasDisabled: GetBool(WmiMappedLogicalDisk.QuotasDisabled),
          QuotasIncomplete: GetBool(WmiMappedLogicalDisk.QuotasIncomplete),
          QuotasRebuilding: GetBool(WmiMappedLogicalDisk.QuotasRebuilding),
          SessionID: GetStr(WmiMappedLogicalDisk.SessionID),
          Size: GetULong(WmiMappedLogicalDisk.Size),
          Status: GetStr(WmiMappedLogicalDisk.Status),
          StatusInfo: (ushort?)GetInt(WmiMappedLogicalDisk.StatusInfo),
          SupportsDiskQuotas: GetBool(WmiMappedLogicalDisk.SupportsDiskQuotas),
          SupportsFileBasedCompression: GetBool(WmiMappedLogicalDisk.SupportsFileBasedCompression),
          SystemCreationClassName: GetStr(WmiMappedLogicalDisk.SystemCreationClassName),
          SystemName: GetStr(WmiMappedLogicalDisk.SystemName),
          VolumeName: GetStr(WmiMappedLogicalDisk.VolumeName),
          VolumeSerialNumber: GetStr(WmiMappedLogicalDisk.VolumeSerialNumber)));
      }
      return results;
    }
    catch {
      return Array.Empty<MappedLogicalDiskMetrics>();
    }
  }
}
