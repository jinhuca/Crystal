using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.IDEController;
public static class WmiIDEControllerExtensions {
  private const string WmiClassName = WmiIDEController.ClassName;

  public static async Task<IReadOnlyList<IDEControllerMetrics>> ToSafeIDEControllerMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance IDE controller data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<IDEControllerMetrics>();
      }

      var results = new List<IDEControllerMetrics>(instancesData.Count);

      // 2. Loop through every detected IDE controller instance sequentially
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
        results.Add(new IDEControllerMetrics(
          Availability: (ushort?)GetInt(WmiIDEController.Availability),
          Caption: GetStr(WmiIDEController.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiIDEController.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiIDEController.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiIDEController.CreationClassName),
          Description: GetStr(WmiIDEController.Description),
          DeviceID: GetStr(WmiIDEController.DeviceID),
          ErrorCleared: GetBool(WmiIDEController.ErrorCleared),
          ErrorDescription: GetStr(WmiIDEController.ErrorDescription),
          InstallDate: GetDate(WmiIDEController.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiIDEController.LastErrorCode),
          Manufacturer: GetStr(WmiIDEController.Manufacturer),
          MaxNumberControlled: (uint?)GetInt(WmiIDEController.MaxNumberControlled),
          Name: GetStr(WmiIDEController.Name),
          PNPDeviceID: GetStr(WmiIDEController.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiIDEController.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiIDEController.PowerManagementSupported),
          ProtocolSupported: (ushort?)GetInt(WmiIDEController.ProtocolSupported),
          Status: GetStr(WmiIDEController.Status),
          StatusInfo: (ushort?)GetInt(WmiIDEController.StatusInfo),
          SystemCreationClassName: GetStr(WmiIDEController.SystemCreationClassName),
          SystemName: GetStr(WmiIDEController.SystemName),
          TimeOfLastReset: GetDate(WmiIDEController.TimeOfLastReset)));
      }
      return results;
    }
    catch {
      return Array.Empty<IDEControllerMetrics>();
    }
  }
}
