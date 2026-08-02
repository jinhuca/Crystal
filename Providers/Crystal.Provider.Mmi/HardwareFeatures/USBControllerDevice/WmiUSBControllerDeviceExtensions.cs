using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.USBControllerDevice;
public static class WmiUSBControllerDeviceExtensions {
  private const string WmiClassName = WmiUSBControllerDevice.ClassName;

  public static async Task<IReadOnlyList<USBControllerDeviceMetrics>> ToSafeUSBControllerDeviceMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance USB controller/device relationship data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<USBControllerDeviceMetrics>();
      }

      var results = new List<USBControllerDeviceMetrics>(instancesData.Count);

      // 2. Loop through every detected relationship instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;
        int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int
          ? v.AsInt() : null;
        ulong? GetULong(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.ULong
          ? v.AsReadOnlyULong() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new USBControllerDeviceMetrics(
          AccessState: (ushort?)GetInt(WmiUSBControllerDevice.AccessState),
          Antecedent: GetStr(WmiUSBControllerDevice.Antecedent),
          Dependent: GetStr(WmiUSBControllerDevice.Dependent),
          NegotiatedDataWidth: (uint?)GetInt(WmiUSBControllerDevice.NegotiatedDataWidth),
          NegotiatedSpeed: GetULong(WmiUSBControllerDevice.NegotiatedSpeed),
          NumberOfHardResets: (uint?)GetInt(WmiUSBControllerDevice.NumberOfHardResets),
          NumberOfSoftResets: (uint?)GetInt(WmiUSBControllerDevice.NumberOfSoftResets)));
      }
      return results;
    }
    catch {
      return Array.Empty<USBControllerDeviceMetrics>();
    }
  }
}
