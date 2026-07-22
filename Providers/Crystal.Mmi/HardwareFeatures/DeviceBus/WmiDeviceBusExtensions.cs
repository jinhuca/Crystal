using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.DeviceBus;
public static class WmiDeviceBusExtensions {
  private const string WmiClassName = WmiDeviceBus.ClassName;

  public static async Task<IReadOnlyList<DeviceBusMetrics>> ToSafeDeviceBusMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance association data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<DeviceBusMetrics>();
      }

      var results = new List<DeviceBusMetrics>(instancesData.Count);

      // 2. Loop through every detected bus/device relationship instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new DeviceBusMetrics(
          Antecedent: GetStr(WmiDeviceBus.Antecedent),
          Dependent: GetStr(WmiDeviceBus.Dependent)));
      }
      return results;
    }
    catch {
      return Array.Empty<DeviceBusMetrics>();
    }
  }
}
