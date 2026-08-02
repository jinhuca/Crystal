using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.Keyboard;
public static class WmiKeyboardExtensions {
  private const string WmiClassName = WmiKeyboard.ClassName;

  public static async Task<IReadOnlyList<KeyboardMetrics>> ToSafeKeyboardMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance keyboard data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<KeyboardMetrics>();
      }

      var results = new List<KeyboardMetrics>(instancesData.Count);

      // 2. Loop through every detected keyboard instance sequentially
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
        results.Add(new KeyboardMetrics(
          Availability: (ushort?)GetInt(WmiKeyboard.Availability),
          Caption: GetStr(WmiKeyboard.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiKeyboard.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiKeyboard.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiKeyboard.CreationClassName),
          Description: GetStr(WmiKeyboard.Description),
          DeviceID: GetStr(WmiKeyboard.DeviceID),
          ErrorCleared: GetBool(WmiKeyboard.ErrorCleared),
          ErrorDescription: GetStr(WmiKeyboard.ErrorDescription),
          InstallDate: GetDate(WmiKeyboard.InstallDate),
          IsLocked: GetBool(WmiKeyboard.IsLocked),
          LastErrorCode: (uint?)GetInt(WmiKeyboard.LastErrorCode),
          Layout: GetStr(WmiKeyboard.Layout),
          Name: GetStr(WmiKeyboard.Name),
          NumberOfFunctionKeys: (ushort?)GetInt(WmiKeyboard.NumberOfFunctionKeys),
          Password: (ushort?)GetInt(WmiKeyboard.Password),
          PNPDeviceID: GetStr(WmiKeyboard.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiKeyboard.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiKeyboard.PowerManagementSupported),
          Status: GetStr(WmiKeyboard.Status),
          StatusInfo: (ushort?)GetInt(WmiKeyboard.StatusInfo),
          SystemCreationClassName: GetStr(WmiKeyboard.SystemCreationClassName),
          SystemName: GetStr(WmiKeyboard.SystemName)));
      }
      return results;
    }
    catch {
      return Array.Empty<KeyboardMetrics>();
    }
  }
}
