using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.DiskDrive;

/// <summary>
/// Provides extension methods for <see cref="IWmiHardwareProvider"/> to read disk drive metrics from WMI 
/// (<c>Win32_DiskDrive</c>) and convert them into safe, null-tolerant <see cref="DiskDriveMetrics"/> objects.
/// </summary>
public static class WmiDiskExtensions {
  /// <summary>
  /// The WMI class name for physical disk drives, used to query the WMI provider for disk drive metrics.
  /// </summary>
  private const string WmiClassName = WmiDiskDrive.ClassName;

  /// <summary>
  /// Asynchronously queries the WMI provider for physical disk drive metrics and converts them into a list 
  /// of <see cref="DiskDriveMetrics"/> objects.
  /// </summary>
  /// <param name="provider">The WMI hardware provider.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <param name="bypassCache">A value indicating whether to bypass the cache.</param>
  /// <returns>A task representing the asynchronous operation.</returns>

  public static async Task<IReadOnlyList<DiskDriveMetrics>> ToSafeDiskDriveMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken,
    bool bypassCache = false) {
    try {
      // 1. Fetch multi-instance driver data blocks asynchronously. Callers that re-enumerate to
      //    catch a hotplug (a USB drive attached/removed) must pass bypassCache: true, otherwise the
      //    provider's per-class cache would keep returning the drive set seen at first query.
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken, bypassCache);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<DiskDriveMetrics>();
      }

      var results = new List<DiskDriveMetrics>(instancesData.Count);

      // 2. Loop through every detected physical drive instance
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
        string[]? GetStrArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.StringArray 
          ? v.AsStringArray() : null;
        ushort[]? GetUShortArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.UShortArray 
          ? v.AsUShortArray() : null;
        ulong? GetULong(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.ULong 
          ? v.AsReadOnlyULong() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new DiskDriveMetrics(
          Availability: (ushort?)GetInt(WmiDiskDrive.Availability),
          BytesPerSector: (uint?)GetInt(WmiDiskDrive.BytesPerSector),
          Capabilities: GetUShortArr(WmiDiskDrive.Capabilities),
          CapabilityDescriptions: GetStrArr(WmiDiskDrive.CapabilityDescriptions),
          Caption: GetStr(WmiDiskDrive.Caption),
          CompressionMethod: GetStr(WmiDiskDrive.CompressionMethod),
          ConfigManagerErrorCode: (uint?)GetInt(WmiDiskDrive.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiDiskDrive.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiDiskDrive.CreationClassName),
          DefaultBlockSize: GetULong(WmiDiskDrive.DefaultBlockSize),
          Description: GetStr(WmiDiskDrive.Description),
          DeviceID: GetStr(WmiDiskDrive.DeviceID),
          ErrorCleared: GetBool(WmiDiskDrive.ErrorCleared),
          ErrorDescription: GetStr(WmiDiskDrive.ErrorDescription),
          ErrorMethodology: GetStr(WmiDiskDrive.ErrorMethodology),
          FirmwareRevision: GetStr(WmiDiskDrive.FirmwareRevision),
          Index: (uint?)GetInt(WmiDiskDrive.Index),
          InstallDate: GetDate(WmiDiskDrive.InstallDate),
          InterfaceType: GetStr(WmiDiskDrive.InterfaceType),
          LastErrorCode: (uint?)GetInt(WmiDiskDrive.LastErrorCode),
          Manufacturer: GetStr(WmiDiskDrive.Manufacturer),
          MaxBlockSize: GetULong(WmiDiskDrive.MaxBlockSize),
          MaxMediaSize: GetULong(WmiDiskDrive.MaxMediaSize),
          MediaLoaded: GetBool(WmiDiskDrive.MediaLoaded),
          MediaType: GetStr(WmiDiskDrive.MediaType),
          MinBlockSize: GetULong(WmiDiskDrive.MinBlockSize),
          Model: GetStr(WmiDiskDrive.Model),
          Name: GetStr(WmiDiskDrive.Name),
          NeedsCleaning: GetBool( WmiDiskDrive.NeedsCleaning),
          NumberOfMediaSupported: (uint?)GetInt(WmiDiskDrive.NumberOfMediaSupported),
          Partitions: (uint?)GetInt(WmiDiskDrive.Partitions),
          PNPDeviceID: GetStr(WmiDiskDrive.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiDiskDrive.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiDiskDrive.PowerManagementSupported),
          SCSIBus: (uint?)GetInt(WmiDiskDrive.SCSIBus),
          SCSILogicalUnit: (ushort?)GetInt(WmiDiskDrive.SCSILogicalUnit),
          SCSIPort: (ushort?)GetInt(WmiDiskDrive.SCSIPort),
          SCSITargetId: (ushort?)GetInt(WmiDiskDrive.SCSITargetId),
          SectorsPerTrack: (uint?)GetInt(WmiDiskDrive.SectorsPerTrack),
          SerialNumber: GetStr(WmiDiskDrive.SerialNumber),
          Signature: (uint?)GetInt(WmiDiskDrive.Signature),
          Size: GetULong(WmiDiskDrive.Size),
          Status: GetStr(WmiDiskDrive.Status),
          StatusInfo: (ushort?)GetInt(WmiDiskDrive.StatusInfo),
          SystemCreationClassName: GetStr(WmiDiskDrive.SystemCreationClassName),
          SystemName: GetStr(WmiDiskDrive.SystemName),
          TotalCylinders: GetULong(WmiDiskDrive.TotalCylinders),
          TotalHeads: (uint?)GetInt(WmiDiskDrive.TotalHeads),
          TotalSectors: GetULong(WmiDiskDrive.TotalSectors),
          TotalTracks: GetULong(WmiDiskDrive.TotalTracks),
          TracksPerCylinder: (uint?)GetInt(WmiDiskDrive.TracksPerCylinder)));
      }

      return results;
    }
    catch {
      return Array.Empty<DiskDriveMetrics>();
    }
  }
}
