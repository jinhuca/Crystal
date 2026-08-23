using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.AssociatedProcessorMemory;

/// <summary>
/// Provides extension methods for <see cref="IWmiHardwareProvider"/> to read the
/// <see cref="AssociatedProcessorMemoryMetrics"/> from WMI.
/// </summary>
public static class WmiAssociatedProcessorMemoryExtensions {
  /// <summary>
  /// The WMI class name for the associated processor memory relationship.
  /// </summary>
  private const string WmiClassName = WmiAssociatedProcessorMemory.ClassName;

  /// <summary>
  /// Asynchronously retrieves the associated processor memory metrics from WMI using the provided hardware provider.
  /// </summary>
  /// <param name="provider">The WMI hardware provider.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>A task that represents the asynchronous operation and returns a list of associated processor memory metrics.</returns>
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
