using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.DeviceBus;

/// <summary>
/// Provides extension methods for <see cref="IWmiHardwareProvider"/> to read device-bus association data from WMI
/// (<c>Win32_DeviceBus</c>) and convert it into safe, null-tolerant <see cref="DeviceBusMetrics"/> instances.
/// </summary>
public static class WmiDeviceBusExtensions {
  /// <summary>
  /// The WMI class name for device-bus association data (<c>Win32_DeviceBus</c>).
  /// </summary>
  private const string WmiClassName = WmiDeviceBus.ClassName;

  /// <summary>
  /// Asynchronously retrieves device-bus association metrics from WMI and converts them into a list of 
  /// <see cref="DeviceBusMetrics"/> instances.
  /// </summary>
  /// <param name="provider">The WMI hardware provider.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
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
