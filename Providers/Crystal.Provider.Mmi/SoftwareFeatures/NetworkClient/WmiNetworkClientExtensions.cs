using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.SoftwareFeatures.NetworkClient;
public static class WmiNetworkClientExtensions {
  private const string WmiClassName = WmiNetworkClient.ClassName;

  public static async Task<IReadOnlyList<NetworkClientMetrics>> ToSafeNetworkClientMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance network client data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<NetworkClientMetrics>();
      }

      var results = new List<NetworkClientMetrics>(instancesData.Count);

      // 2. Loop through every detected network client instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;
        DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime
          ? v.AsDateTime() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new NetworkClientMetrics(
          Caption: GetStr(WmiNetworkClient.Caption),
          Description: GetStr(WmiNetworkClient.Description),
          InstallDate: GetDate(WmiNetworkClient.InstallDate),
          Manufacturer: GetStr(WmiNetworkClient.Manufacturer),
          Name: GetStr(WmiNetworkClient.Name),
          Status: GetStr(WmiNetworkClient.Status)));
      }
      return results;
    }
    catch {
      return Array.Empty<NetworkClientMetrics>();
    }
  }
}
