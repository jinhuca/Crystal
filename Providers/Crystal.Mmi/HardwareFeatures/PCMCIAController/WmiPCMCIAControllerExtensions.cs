using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.PCMCIAController;
public static class WmiPCMCIAControllerExtensions {
  private const string WmiClassName = WmiPCMCIAController.ClassName;

  public static async Task<IReadOnlyList<PCMCIAControllerMetrics>> ToSafePCMCIAControllerMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance PCMCIA controller data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<PCMCIAControllerMetrics>();
      }

      var results = new List<PCMCIAControllerMetrics>(instancesData.Count);

      // 2. Loop through every detected PCMCIA controller instance sequentially
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
        results.Add(new PCMCIAControllerMetrics(
          Availability: (ushort?)GetInt(WmiPCMCIAController.Availability),
          Caption: GetStr(WmiPCMCIAController.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiPCMCIAController.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiPCMCIAController.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiPCMCIAController.CreationClassName),
          Description: GetStr(WmiPCMCIAController.Description),
          DeviceID: GetStr(WmiPCMCIAController.DeviceID),
          ErrorCleared: GetBool(WmiPCMCIAController.ErrorCleared),
          ErrorDescription: GetStr(WmiPCMCIAController.ErrorDescription),
          InstallDate: GetDate(WmiPCMCIAController.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiPCMCIAController.LastErrorCode),
          Manufacturer: GetStr(WmiPCMCIAController.Manufacturer),
          MaxNumberControlled: (uint?)GetInt(WmiPCMCIAController.MaxNumberControlled),
          Name: GetStr(WmiPCMCIAController.Name),
          PNPDeviceID: GetStr(WmiPCMCIAController.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiPCMCIAController.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiPCMCIAController.PowerManagementSupported),
          ProtocolSupported: (ushort?)GetInt(WmiPCMCIAController.ProtocolSupported),
          Status: GetStr(WmiPCMCIAController.Status),
          StatusInfo: (ushort?)GetInt(WmiPCMCIAController.StatusInfo),
          SystemCreationClassName: GetStr(WmiPCMCIAController.SystemCreationClassName),
          SystemName: GetStr(WmiPCMCIAController.SystemName),
          TimeOfLastReset: GetDate(WmiPCMCIAController.TimeOfLastReset)));
      }
      return results;
    }
    catch {
      return Array.Empty<PCMCIAControllerMetrics>();
    }
  }
}
