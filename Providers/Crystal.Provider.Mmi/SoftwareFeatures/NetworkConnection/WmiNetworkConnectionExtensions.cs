using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.SoftwareFeatures.NetworkConnection;
public static class WmiNetworkConnectionExtensions {
  private const string WmiClassName = WmiNetworkConnection.ClassName;

  public static async Task<IReadOnlyList<NetworkConnectionMetrics>> ToSafeNetworkConnectionMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance network connection data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<NetworkConnectionMetrics>();
      }

      var results = new List<NetworkConnectionMetrics>(instancesData.Count);

      // 2. Loop through every detected network connection instance sequentially
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

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new NetworkConnectionMetrics(
          AccessMask: (uint?)GetInt(WmiNetworkConnection.AccessMask),
          Caption: GetStr(WmiNetworkConnection.Caption),
          Comment: GetStr(WmiNetworkConnection.Comment),
          ConnectionState: GetStr(WmiNetworkConnection.ConnectionState),
          ConnectionType: GetStr(WmiNetworkConnection.ConnectionType),
          Description: GetStr(WmiNetworkConnection.Description),
          DisplayType: GetStr(WmiNetworkConnection.DisplayType),
          InstallDate: GetDate(WmiNetworkConnection.InstallDate),
          LocalName: GetStr(WmiNetworkConnection.LocalName),
          Name: GetStr(WmiNetworkConnection.Name),
          Persistent: GetBool(WmiNetworkConnection.Persistent),
          ProviderName: GetStr(WmiNetworkConnection.ProviderName),
          RemoteName: GetStr(WmiNetworkConnection.RemoteName),
          RemotePath: GetStr(WmiNetworkConnection.RemotePath),
          ResourceType: GetStr(WmiNetworkConnection.ResourceType),
          Status: GetStr(WmiNetworkConnection.Status),
          UserName: GetStr(WmiNetworkConnection.UserName)));
      }
      return results;
    }
    catch {
      return Array.Empty<NetworkConnectionMetrics>();
    }
  }
}
