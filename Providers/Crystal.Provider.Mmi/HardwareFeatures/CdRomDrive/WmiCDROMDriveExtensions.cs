using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.CdRomDrive;

public static class WmiCDROMDriveExtensions {
  private const string WmiClassName = WmiCDROMDrive.ClassName;

  public static async Task<IReadOnlyList<CDROMDriveMetrics>> ToSafeCDROMDriveMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance runtime CD-ROM drive data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<CDROMDriveMetrics>();
      }

      var results = new List<CDROMDriveMetrics>(instancesData.Count);

      // 2. Loop through every single detected optical drive
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
        results.Add(new CDROMDriveMetrics(
          Availability: (ushort?)GetInt(WmiCDROMDrive.Availability),
          CapabilityDescriptions: GetStrArr(WmiCDROMDrive.CapabilityDescriptions),
          Caption: GetStr(WmiCDROMDrive.Caption),
          CompressionMethod: GetStr(WmiCDROMDrive.CompressionMethod),
          ConfigManagerErrorCode: (uint?)GetInt(WmiCDROMDrive.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiCDROMDrive.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiCDROMDrive.CreationClassName),
          DefaultBlockSize: GetULong(WmiCDROMDrive.DefaultBlockSize),
          Description: GetStr(WmiCDROMDrive.Description),
          DeviceID: GetStr(WmiCDROMDrive.DeviceID),
          Drive: GetStr(WmiCDROMDrive.Drive),
          DriveIntegrity: GetBool(WmiCDROMDrive.DriveIntegrity),
          ErrorCleared: GetBool(WmiCDROMDrive.ErrorCleared),
          ErrorDescription: GetStr(WmiCDROMDrive.ErrorDescription),
          ErrorMethodology: GetStr(WmiCDROMDrive.ErrorMethodology),
          FileSystemFlags: (ushort?)GetInt(WmiCDROMDrive.FileSystemFlags),
          FileSystemFlagsEx: (uint?)GetInt(WmiCDROMDrive.FileSystemFlagsEx),
          Id: GetStr(WmiCDROMDrive.Id),
          InstallDate: GetDate(WmiCDROMDrive.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiCDROMDrive.LastErrorCode),
          Manufacturer: GetStr(WmiCDROMDrive.Manufacturer),
          MaxBlockSize: GetULong(WmiCDROMDrive.MaxBlockSize),
          MaximumComponentLength: (uint?)GetInt(WmiCDROMDrive.MaximumComponentLength),
          MaxMediaSize: GetULong(WmiCDROMDrive.MaxMediaSize),
          MediaLoaded: GetBool(WmiCDROMDrive.MediaLoaded),
          MediaType: GetStr(WmiCDROMDrive.MediaType),
          MfrAssignedRevisionLevel: GetStr(WmiCDROMDrive.MfrAssignedRevisionLevel),
          MinBlockSize: GetULong(WmiCDROMDrive.MinBlockSize),
          Name: GetStr(WmiCDROMDrive.Name),
          NeedsCleaning: GetBool(WmiCDROMDrive.NeedsCleaning),
          NumberOfMediaSupported: (uint?)GetInt(WmiCDROMDrive.NumberOfMediaSupported),
          PNPDeviceID: GetStr(WmiCDROMDrive.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiCDROMDrive.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiCDROMDrive.PowerManagementSupported),
          RevisionLevel: GetStr(WmiCDROMDrive.RevisionLevel),
          SCSIBus: (uint?)GetInt(WmiCDROMDrive.SCSIBus),
          SCSILogicalUnit: (ushort?)GetInt(WmiCDROMDrive.SCSILogicalUnit),
          SCSIPort: (ushort?)GetInt(WmiCDROMDrive.SCSIPort),
          SCSITargetId: (ushort?)GetInt(WmiCDROMDrive.SCSITargetId),
          SerialNumber: GetStr(WmiCDROMDrive.SerialNumber),
          Size: GetULong(WmiCDROMDrive.Size),
          Status: GetStr(WmiCDROMDrive.Status),
          StatusInfo: (ushort?)GetInt(WmiCDROMDrive.StatusInfo),
          SystemCreationClassName: GetStr(WmiCDROMDrive.SystemCreationClassName),
          SystemName: GetStr(WmiCDROMDrive.SystemName),
          VolumeName: GetStr(WmiCDROMDrive.VolumeName),
          VolumeSerialNumber: GetStr(WmiCDROMDrive.VolumeSerialNumber)));
      }
      return results;
    }
    catch {
      return Array.Empty<CDROMDriveMetrics>();
    }
  }
}
