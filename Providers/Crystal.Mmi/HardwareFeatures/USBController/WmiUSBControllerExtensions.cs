using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.USBController;

public static class WmiUSBControllerExtensions {
  private const string WmiClassName = WmiUSBController.ClassName;

  public static async Task<IReadOnlyList<USBControllerMetrics>> ToSafeUSBControllerMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance data collection asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if(instancesData == null || instancesData.Count == 0) {
        return Array.Empty<USBControllerMetrics>();
      }

      var results = new List<USBControllerMetrics>(instancesData.Count);

      foreach(var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN LOOKUP CONDITIONAL WRAPPERS ---
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

        // --- INSTANTIATE SORTED EXTRACTED VALUES ---
        results.Add(new USBControllerMetrics(
          Availability: (ushort?)GetInt(WmiUSBController.Availability),
          Caption: GetStr(WmiUSBController.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiUSBController.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiUSBController.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiUSBController.CreationClassName),
          Description: GetStr(WmiUSBController.Description),
          DeviceID: GetStr(WmiUSBController.DeviceID),
          ErrorCleared: GetBool(WmiUSBController.ErrorCleared),
          ErrorDescription: GetStr(WmiUSBController.ErrorDescription),
          InstallDate: GetDate(WmiUSBController.InstallationDate),
          LastErrorCode: (uint?)GetInt(WmiUSBController.LastErrorCode),
          Manufacturer: GetStr(WmiUSBController.Manufacturer),
          MaxNumberControlled: (uint?)GetInt(WmiUSBController.MaxNumberControlled),
          Name: GetStr(WmiUSBController.Name),
          PNPDeviceID: GetStr(WmiUSBController.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiUSBController.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiUSBController.PowerManagementSupported),
          ProtocolSupported: (ushort?)GetInt(WmiUSBController.ProtocolSupported),
          Status: GetStr(WmiUSBController.Status),
          StatusInfo: (ushort?)GetInt(WmiUSBController.StatusInfo),
          SystemCreationClassName: GetStr(WmiUSBController.SystemCreationClassName),
          SystemName: GetStr(WmiUSBController.SystemName),
          TimeOfLastReset: GetDate(WmiUSBController.TimeOfLastReset)));
      }
      return results;
    }
    catch {
      return Array.Empty<USBControllerMetrics>();
    }
  }
}
