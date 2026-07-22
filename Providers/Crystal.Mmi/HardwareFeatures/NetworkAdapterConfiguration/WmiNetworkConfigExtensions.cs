using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.NetworkAdapterConfiguration;

public static class WmiNetworkConfigExtensions {
  private const string WmiClassName = WmiNetworkAdapterConfiguration.ClassName;

  public static async Task<IReadOnlyList<NetworkAdapterConfigMetrics>> ToSafeNetworkAdapterConfigMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if(instancesData == null || instancesData.Count == 0) {
        return Array.Empty<NetworkAdapterConfigMetrics>();
      }

      var results = new List<NetworkAdapterConfigMetrics>(instancesData.Count);

      foreach(var data in instancesData) {
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
        string? FlattenStrArray(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.StringArray 
          ? string.Join(", ", v.AsStringArray() ?? Array.Empty<string>()) : null;

        // Only evaluate logical adapter configs that are actively running TCP/IP bounds
        bool ipEnabled = GetBool("IPEnabled") ?? false;
        if(!ipEnabled) continue;

        results.Add(new NetworkAdapterConfigMetrics(
            ArpAlwaysSourceRoute: GetBool(WmiNetworkAdapterConfiguration.ArpAlwaysSourceRoute),
            ArpUseEtherSNAP: GetBool(WmiNetworkAdapterConfiguration.ArpUseEtherSNAP),
            Caption: GetStr(WmiNetworkAdapterConfiguration.Caption),
            DatabasePath: GetStr(WmiNetworkAdapterConfiguration.DatabasePath),
            DeadGWDetectEnabled: GetBool(WmiNetworkAdapterConfiguration.DeadGWDetectEnabled),
            DefaultIPGateway: FlattenStrArray(WmiNetworkAdapterConfiguration.DefaultIPGateway),
            DefaultTTL: (uint?)GetInt(WmiNetworkAdapterConfiguration.DefaultTTL),
            Description: GetStr(WmiNetworkAdapterConfiguration.Description),
            DHCPEnabled: GetBool(WmiNetworkAdapterConfiguration.DHCPEnabled),
            DHCPLeaseExpires: GetDate(WmiNetworkAdapterConfiguration.DHCPLeaseExpires),
            DHCPLeaseObtained: GetDate(WmiNetworkAdapterConfiguration.DHCPLeaseObtained),
            DHCPServer: GetStr(WmiNetworkAdapterConfiguration.DHCPServer),
            DNSDomain: GetStr(WmiNetworkAdapterConfiguration.DNSDomain),
            DNSDomainSuffixSearchOrder: FlattenStrArray(WmiNetworkAdapterConfiguration.DNSDomainSuffixSearchOrder),
            DNSEnabledForWINSResolution: GetBool(WmiNetworkAdapterConfiguration.DNSEnabledForWINSResolution),
            DNSHostName: GetStr(WmiNetworkAdapterConfiguration.DNSHostName),
            DNSServerSearchOrder: FlattenStrArray(WmiNetworkAdapterConfiguration.DNSServerSearchOrder),
            DomainDNSRegistrationEnabled: GetBool(WmiNetworkAdapterConfiguration.DomainDNSRegistrationEnabled),
            ForwardBufferMemory: (uint?)GetInt(WmiNetworkAdapterConfiguration.ForwardBufferMemory),
            FullDNSRegistrationEnabled: GetBool(WmiNetworkAdapterConfiguration.FullDNSRegistrationEnabled),
            IGMPLevel: (uint?)GetInt(WmiNetworkAdapterConfiguration.IGMPLevel),
            Index: (uint?)GetInt(WmiNetworkAdapterConfiguration.Index),
            InterfaceIndex: (uint?)GetInt(WmiNetworkAdapterConfiguration.InterfaceIndex),
            IPAddress: FlattenStrArray(WmiNetworkAdapterConfiguration.IPAddress),
            IPConnectionMetric: (uint?)GetInt(WmiNetworkAdapterConfiguration.IPConnectionMetric),
            IPEnabled: ipEnabled,
            IPFilterSecurityEnabled: GetBool(WmiNetworkAdapterConfiguration.IPFilterSecurityEnabled),
            IPSubnet: FlattenStrArray(WmiNetworkAdapterConfiguration.IPSubnet),
            KeepAliveInterval: (uint?)GetInt(WmiNetworkAdapterConfiguration.KeepAliveInterval),
            KeepAliveTime: (uint?)GetInt(WmiNetworkAdapterConfiguration.KeepAliveTime),
            MACAddress: GetStr(WmiNetworkAdapterConfiguration.MACAddress),
            MTU: (uint?)GetInt(WmiNetworkAdapterConfiguration.MTU),
            NumForwardPackets: (uint?)GetInt(WmiNetworkAdapterConfiguration.NumForwardPackets),
            PMTUDiscoveryEnabled: GetBool(WmiNetworkAdapterConfiguration.PMTUDiscoveryEnabled),
            PMTUBHDetectEnabled: GetBool(WmiNetworkAdapterConfiguration.PMTUBHDetectEnabled),
            ProviderName: GetStr(WmiNetworkAdapterConfiguration.ProviderName),
            RegistryKeyByDNS: GetBool(WmiNetworkAdapterConfiguration.RegistryKeyByDNS),
            SettingID: GetStr(WmiNetworkAdapterConfiguration.SettingID),
            SystemCreationClassName: GetStr(WmiNetworkAdapterConfiguration.SystemCreationClassName),
            SystemName: GetStr(WmiNetworkAdapterConfiguration.SystemName),
            TcpipNetbiosOptions: GetBool(WmiNetworkAdapterConfiguration.TcpipNetbiosOptions),
            TcpMaxConnectRetransmissions: (uint?)GetInt(WmiNetworkAdapterConfiguration.TcpMaxConnectRetransmissions),
            TcpMaxDataRetransmissions: (uint?)GetInt(WmiNetworkAdapterConfiguration.TcpMaxDataRetransmissions),
            TcpNumConnections: (uint?)GetInt(WmiNetworkAdapterConfiguration.TcpNumConnections),
            TcpWindowSize: (uint?)GetInt(WmiNetworkAdapterConfiguration.TcpWindowSize),
            UseZeroBroadcast: GetBool(WmiNetworkAdapterConfiguration.UseZeroBroadcast),
            WinsDownControl: GetBool(WmiNetworkAdapterConfiguration.WinsDownControl),
            WINSHostLookupFile: GetStr(WmiNetworkAdapterConfiguration.WINSHostLookupFile),
            WINSPrimaryServer: GetStr(WmiNetworkAdapterConfiguration.WINSPrimaryServer),
            WINSScopeID: GetStr(WmiNetworkAdapterConfiguration.WINSScopeID),
            WINSSecondaryServer: GetStr(WmiNetworkAdapterConfiguration.WINSSecondaryServer)
        ));
      }
      return results;
    }
    catch {
      return Array.Empty<NetworkAdapterConfigMetrics>();
    }
  }
}
