#nullable enable
namespace Crystal.Mmi.HardwareFeatures.NetworkAdapterConfiguration;
public record NetworkAdapterConfigMetrics(
    bool? ArpAlwaysSourceRoute,
    bool? ArpUseEtherSNAP,
    string? Caption,
    string? DatabasePath,
    bool? DeadGWDetectEnabled,
    string? DefaultIPGateway,         // Flattened array string of gateways
    uint? DefaultTTL,
    string? Description,
    bool? DHCPEnabled,
    DateTime? DHCPLeaseExpires,
    DateTime? DHCPLeaseObtained,
    string? DHCPServer,
    string? DNSDomain,
    string? DNSDomainSuffixSearchOrder, // Flattened array string
    bool? DNSEnabledForWINSResolution,
    string? DNSHostName,
    string? DNSServerSearchOrder,       // Flattened array string
    bool? DomainDNSRegistrationEnabled,
    uint? ForwardBufferMemory,
    bool? FullDNSRegistrationEnabled,
    uint? IGMPLevel,
    uint? Index,                        // The WMI tracking index configuration index
    uint? InterfaceIndex,                // The cross-reference key linking to Win32_NetworkAdapter
    string? IPAddress,                  // Flattened array string of IPv4/IPv6 allocations
    uint? IPConnectionMetric,
    bool? IPEnabled,                    // True if TCP/IP is active on this adapter interface
    bool? IPFilterSecurityEnabled,
    string? IPSubnet,                   // Flattened array string of masks
    uint? KeepAliveInterval,
    uint? KeepAliveTime,
    string? MACAddress,
    uint? MTU,
    uint? NumForwardPackets,
    bool? PMTUDiscoveryEnabled,
    bool? PMTUBHDetectEnabled,
    string? ProviderName,
    bool? RegistryKeyByDNS,
    string? SettingID,
    string? SystemCreationClassName,
    string? SystemName,
    bool? TcpipNetbiosOptions,
    uint? TcpMaxConnectRetransmissions,
    uint? TcpMaxDataRetransmissions,
    uint? TcpNumConnections,
    uint? TcpWindowSize,
    bool? UseZeroBroadcast,
    bool? WinsDownControl,
    string? WINSHostLookupFile,
    string? WINSPrimaryServer,
    string? WINSScopeID,
    string? WINSSecondaryServer
);
