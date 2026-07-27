using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.FloppyController;
public static class WmiFloppyControllerExtensions {
  private const string WmiClassName = WmiFloppyController.ClassName;

  public static async Task<IReadOnlyList<FloppyControllerMetrics>> ToSafeFloppyControllerMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance floppy controller data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<FloppyControllerMetrics>();
      }

      var results = new List<FloppyControllerMetrics>(instancesData.Count);

      // 2. Loop through every detected floppy controller instance sequentially
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
        results.Add(new FloppyControllerMetrics(
          Availability: (ushort?)GetInt(WmiFloppyController.Availability),
          Caption: GetStr(WmiFloppyController.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiFloppyController.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiFloppyController.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiFloppyController.CreationClassName),
          Description: GetStr(WmiFloppyController.Description),
          DeviceID: GetStr(WmiFloppyController.DeviceID),
          ErrorCleared: GetBool(WmiFloppyController.ErrorCleared),
          ErrorDescription: GetStr(WmiFloppyController.ErrorDescription),
          InstallDate: GetDate(WmiFloppyController.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiFloppyController.LastErrorCode),
          Manufacturer: GetStr(WmiFloppyController.Manufacturer),
          MaxNumberControlled: (uint?)GetInt(WmiFloppyController.MaxNumberControlled),
          Name: GetStr(WmiFloppyController.Name),
          PNPDeviceID: GetStr(WmiFloppyController.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiFloppyController.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiFloppyController.PowerManagementSupported),
          ProtocolSupported: (ushort?)GetInt(WmiFloppyController.ProtocolSupported),
          Status: GetStr(WmiFloppyController.Status),
          StatusInfo: (ushort?)GetInt(WmiFloppyController.StatusInfo),
          SystemCreationClassName: GetStr(WmiFloppyController.SystemCreationClassName),
          SystemName: GetStr(WmiFloppyController.SystemName),
          TimeOfLastReset: GetDate(WmiFloppyController.TimeOfLastReset)));
      }
      return results;
    }
    catch {
      return Array.Empty<FloppyControllerMetrics>();
    }
  }
}
