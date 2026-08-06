using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.IDEControllerDevice;
public static class WmiIDEControllerDeviceExtensions {
  private const string WmiClassName = WmiIDEControllerDevice.ClassName;

  public static async Task<IReadOnlyList<IDEControllerDeviceMetrics>> ToSafeIDEControllerDeviceMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance IDE controller/device relationship data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<IDEControllerDeviceMetrics>();
      }

      var results = new List<IDEControllerDeviceMetrics>(instancesData.Count);

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
        results.Add(new IDEControllerDeviceMetrics(
          AccessState: (ushort?)GetInt(WmiIDEControllerDevice.AccessState),
          Antecedent: GetStr(WmiIDEControllerDevice.Antecedent),
          Dependent: GetStr(WmiIDEControllerDevice.Dependent),
          NegotiatedDataWidth: (uint?)GetInt(WmiIDEControllerDevice.NegotiatedDataWidth),
          NegotiatedSpeed: GetULong(WmiIDEControllerDevice.NegotiatedSpeed),
          NumberOfHardResets: (uint?)GetInt(WmiIDEControllerDevice.NumberOfHardResets),
          NumberOfSoftResets: (uint?)GetInt(WmiIDEControllerDevice.NumberOfSoftResets)));
      }
      return results;
    }
    catch {
      return Array.Empty<IDEControllerDeviceMetrics>();
    }
  }
}
