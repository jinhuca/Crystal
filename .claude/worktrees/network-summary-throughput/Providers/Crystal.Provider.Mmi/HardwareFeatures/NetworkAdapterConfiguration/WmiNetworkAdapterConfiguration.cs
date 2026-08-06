using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.NetworkAdapterConfiguration;

internal static class WmiNetworkAdapterConfiguration {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = "Win32_NetworkAdapterConfiguration";

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;

  // ---------------------------------------------------------------------
  // Network Adapter Configuration Specific Properties
  // ---------------------------------------------------------------------
  public const string ArpAlwaysSourceRoute = nameof(ArpAlwaysSourceRoute);
  public const string ArpUseEtherSNAP = nameof(ArpUseEtherSNAP);
  public const string DatabasePath = nameof(DatabasePath);
  public const string DeadGWDetectEnabled = nameof(DeadGWDetectEnabled);
  public const string DefaultIPGateway = nameof(DefaultIPGateway);
  public const string DefaultTTL = nameof(DefaultTTL);
  public const string DHCPEnabled = nameof(DHCPEnabled);
  public const string DHCPLeaseExpires = nameof(DHCPLeaseExpires);
  public const string DHCPLeaseObtained = nameof(DHCPLeaseObtained);
  public const string DHCPServer = nameof(DHCPServer);
  public const string DNSDomain = nameof(DNSDomain);
  public const string DNSDomainSuffixSearchOrder = nameof(DNSDomainSuffixSearchOrder);
  public const string DNSEnabledForWINSResolution = nameof(DNSEnabledForWINSResolution);
  public const string DNSHostName = nameof(DNSHostName);
  public const string DNSServerSearchOrder = nameof(DNSServerSearchOrder);
  public const string DomainDNSRegistrationEnabled = nameof(DomainDNSRegistrationEnabled);
  public const string ForwardBufferMemory = nameof(ForwardBufferMemory);
  public const string FullDNSRegistrationEnabled = nameof(FullDNSRegistrationEnabled);
  public const string IGMPLevel = nameof(IGMPLevel);
  public const string Index = nameof(Index);
  public const string InterfaceIndex = nameof(InterfaceIndex);
  public const string IPAddress = nameof(IPAddress);
  public const string IPConnectionMetric = nameof(IPConnectionMetric);
  public const string IPEnabled = nameof(IPEnabled);
  public const string IPFilterSecurityEnabled = nameof(IPFilterSecurityEnabled);
  public const string IPSubnet = nameof(IPSubnet);
  public const string KeepAliveInterval = nameof(KeepAliveInterval);
  public const string KeepAliveTime = nameof(KeepAliveTime);
  public const string MACAddress = nameof(MACAddress);
  public const string MTU = nameof(MTU);
  public const string NumForwardPackets = nameof(NumForwardPackets);
  public const string PMTUBHDetectEnabled = nameof(PMTUBHDetectEnabled);
  public const string PMTUDiscoveryEnabled = nameof(PMTUDiscoveryEnabled);
  public const string ProviderName = nameof(ProviderName);
  public const string RegistryKeyByDNS = nameof(RegistryKeyByDNS);
  public const string SettingID = nameof(SettingID);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string TcpipNetbiosOptions = nameof(TcpipNetbiosOptions);
  public const string TcpMaxConnectRetransmissions = nameof(TcpMaxConnectRetransmissions);
  public const string TcpMaxDataRetransmissions = nameof(TcpMaxDataRetransmissions);
  public const string TcpNumConnections = nameof(TcpNumConnections);
  public const string TcpWindowSize = nameof(TcpWindowSize);
  public const string UseZeroBroadcast = nameof(UseZeroBroadcast);
  public const string WinsDownControl = nameof(WinsDownControl);
  public const string WINSHostLookupFile = nameof(WINSHostLookupFile);
  public const string WINSPrimaryServer = nameof(WINSPrimaryServer);
  public const string WINSScopeID = nameof(WINSScopeID);
  public const string WINSSecondaryServer = nameof(WINSSecondaryServer);
}