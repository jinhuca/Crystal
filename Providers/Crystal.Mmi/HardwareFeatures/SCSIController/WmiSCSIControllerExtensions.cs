using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.SCSIController;
public static class WmiSCSIControllerExtensions {
  private const string WmiClassName = WmiSCSIController.ClassName;

  public static async Task<IReadOnlyList<SCSIControllerMetrics>> ToSafeSCSIControllerMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance SCSI controller data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<SCSIControllerMetrics>();
      }

      var results = new List<SCSIControllerMetrics>(instancesData.Count);

      // 2. Loop through every detected SCSI controller instance sequentially
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
        results.Add(new SCSIControllerMetrics(
          Availability: (ushort?)GetInt(WmiSCSIController.Availability),
          Caption: GetStr(WmiSCSIController.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiSCSIController.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiSCSIController.ConfigManagerUserConfig),
          ControllerTimeouts: (uint?)GetInt(WmiSCSIController.ControllerTimeouts),
          CreationClassName: GetStr(WmiSCSIController.CreationClassName),
          Description: GetStr(WmiSCSIController.Description),
          DeviceID: GetStr(WmiSCSIController.DeviceID),
          DeviceMap: GetStr(WmiSCSIController.DeviceMap),
          DriverName: GetStr(WmiSCSIController.DriverName),
          ErrorCleared: GetBool(WmiSCSIController.ErrorCleared),
          ErrorDescription: GetStr(WmiSCSIController.ErrorDescription),
          HardwareVersion: GetStr(WmiSCSIController.HardwareVersion),
          Index: (uint?)GetInt(WmiSCSIController.Index),
          InstallDate: GetDate(WmiSCSIController.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiSCSIController.LastErrorCode),
          Manufacturer: GetStr(WmiSCSIController.Manufacturer),
          MaxDataWidth: (uint?)GetInt(WmiSCSIController.MaxDataWidth),
          MaxNumberControlled: (uint?)GetInt(WmiSCSIController.MaxNumberControlled),
          MaxTransferRate: GetULong(WmiSCSIController.MaxTransferRate),
          Name: GetStr(WmiSCSIController.Name),
          PNPDeviceID: GetStr(WmiSCSIController.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiSCSIController.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiSCSIController.PowerManagementSupported),
          ProtectionManagement: (ushort?)GetInt(WmiSCSIController.ProtectionManagement),
          ProtocolSupported: (ushort?)GetInt(WmiSCSIController.ProtocolSupported),
          Status: GetStr(WmiSCSIController.Status),
          StatusInfo: (ushort?)GetInt(WmiSCSIController.StatusInfo),
          SystemCreationClassName: GetStr(WmiSCSIController.SystemCreationClassName),
          SystemName: GetStr(WmiSCSIController.SystemName),
          TimeOfLastReset: GetDate(WmiSCSIController.TimeOfLastReset)));
      }
      return results;
    }
    catch {
      return Array.Empty<SCSIControllerMetrics>();
    }
  }
}
