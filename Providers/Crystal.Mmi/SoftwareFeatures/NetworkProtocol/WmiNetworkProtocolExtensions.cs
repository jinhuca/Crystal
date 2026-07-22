using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.SoftwareFeatures.NetworkProtocol;
public static class WmiNetworkProtocolExtensions {
  private const string WmiClassName = WmiNetworkProtocol.ClassName;

  public static async Task<IReadOnlyList<NetworkProtocolMetrics>> ToSafeNetworkProtocolMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance network protocol data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<NetworkProtocolMetrics>();
      }

      var results = new List<NetworkProtocolMetrics>(instancesData.Count);

      // 2. Loop through every detected network protocol instance sequentially
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
        results.Add(new NetworkProtocolMetrics(
          Caption: GetStr(WmiNetworkProtocol.Caption),
          ConnectionlessService: GetBool(WmiNetworkProtocol.ConnectionlessService),
          Description: GetStr(WmiNetworkProtocol.Description),
          GuaranteesDelivery: GetBool(WmiNetworkProtocol.GuaranteesDelivery),
          GuaranteesSequencing: GetBool(WmiNetworkProtocol.GuaranteesSequencing),
          InstallDate: GetDate(WmiNetworkProtocol.InstallDate),
          MaximumAddressSize: (uint?)GetInt(WmiNetworkProtocol.MaximumAddressSize),
          MaximumMessageSize: (uint?)GetInt(WmiNetworkProtocol.MaximumMessageSize),
          MessageOriented: GetBool(WmiNetworkProtocol.MessageOriented),
          MinimumAddressSize: (uint?)GetInt(WmiNetworkProtocol.MinimumAddressSize),
          Name: GetStr(WmiNetworkProtocol.Name),
          PseudoStreamOriented: GetBool(WmiNetworkProtocol.PseudoStreamOriented),
          Status: GetStr(WmiNetworkProtocol.Status),
          SupportsBroadcasting: GetBool(WmiNetworkProtocol.SupportsBroadcasting),
          SupportsConnectData: GetBool(WmiNetworkProtocol.SupportsConnectData),
          SupportsDisconnectData: GetBool(WmiNetworkProtocol.SupportsDisconnectData),
          SupportsEncryption: GetBool(WmiNetworkProtocol.SupportsEncryption),
          SupportsExpeditedData: GetBool(WmiNetworkProtocol.SupportsExpeditedData),
          SupportsFragmentation: GetBool(WmiNetworkProtocol.SupportsFragmentation),
          SupportsGracefulClosing: GetBool(WmiNetworkProtocol.SupportsGracefulClosing),
          SupportsGuaranteedBandwidth: GetBool(WmiNetworkProtocol.SupportsGuaranteedBandwidth),
          SupportsMulticasting: GetBool(WmiNetworkProtocol.SupportsMulticasting),
          SupportsQualityofService: GetBool(WmiNetworkProtocol.SupportsQualityofService)));
      }
      return results;
    }
    catch {
      return Array.Empty<NetworkProtocolMetrics>();
    }
  }
}
