using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.AssociatedProcessorMemory;
public static class WmiAssociatedProcessorMemoryExtensions {
  private const string WmiClassName = WmiAssociatedProcessorMemory.ClassName;

  public static async Task<IReadOnlyList<AssociatedProcessorMemoryMetrics>> ToSafeAssociatedProcessorMemoryMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance processor/cache-memory relationship data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<AssociatedProcessorMemoryMetrics>();
      }

      var results = new List<AssociatedProcessorMemoryMetrics>(instancesData.Count);

      // 2. Loop through every detected relationship instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;
        int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int
          ? v.AsInt() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new AssociatedProcessorMemoryMetrics(
          Antecedent: GetStr(WmiAssociatedProcessorMemory.Antecedent),
          BusSpeed: (uint?)GetInt(WmiAssociatedProcessorMemory.BusSpeed),
          Dependent: GetStr(WmiAssociatedProcessorMemory.Dependent)));
      }
      return results;
    }
    catch {
      return Array.Empty<AssociatedProcessorMemoryMetrics>();
    }
  }
}
