using Crystal.Mmi.HardwareFeatures.NetworkAdapterConfiguration;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class NetworkConfigExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> IPEnabledRow(
        string mac = "AA:BB:CC:DD:EE:FF") => WmiRow.Build(
        ("IPEnabled", new WmiValue(true)),
        ("MACAddress", new WmiValue(mac)),
        ("Caption", new WmiValue("[00000001] Intel Ethernet")),
        ("Description", new WmiValue("Intel Ethernet")),
        ("DHCPEnabled", new WmiValue(true)),
        ("DHCPServer", new WmiValue("192.168.1.1")),
        ("DNSHostName", new WmiValue("DESKTOP-01")),
        ("DNSDomain", new WmiValue("local")),
        ("DNSServerSearchOrder", new WmiValue(new[] { "8.8.8.8", "8.8.4.4" })),
        ("DefaultIPGateway", new WmiValue(new[] { "192.168.1.1" })),
        ("IPAddress", new WmiValue(new[] { "192.168.1.100", "::1" })),
        ("IPSubnet", new WmiValue(new[] { "255.255.255.0", "128" })),
        ("Index", new WmiValue(1)),
        ("InterfaceIndex", new WmiValue(10)),
        ("SettingID", new WmiValue("{GUID-1}")),
        ("DatabasePath", new WmiValue(@"%SystemRoot%\System32\drivers\etc")),
        ("DefaultTTL", new WmiValue(128)),
        ("IGMPLevel", new WmiValue(2)),
        ("MTU", new WmiValue(1500)),
        ("TcpWindowSize", new WmiValue(64240)),
        ("TcpMaxDataRetransmissions", new WmiValue(5)),
        ("TcpMaxConnectRetransmissions", new WmiValue(2)),
        ("TcpNumConnections", new WmiValue(0)),
        ("ForwardBufferMemory", new WmiValue(0)),
        ("NumForwardPackets", new WmiValue(0)),
        ("DeadGWDetectEnabled", new WmiValue(true)),
        ("DNSEnabledForWINSResolution", new WmiValue(false)),
        ("DomainDNSRegistrationEnabled", new WmiValue(false)),
        ("FullDNSRegistrationEnabled", new WmiValue(true)),
        ("PMTUDiscoveryEnabled", new WmiValue(true)),
        ("PMTUBHDetectEnabled", new WmiValue(false)),
        ("UseZeroBroadcast", new WmiValue(false)),
        ("WinsDownControl", new WmiValue(false)),
        ("IPFilterSecurityEnabled", new WmiValue(false)),
        ("ArpAlwaysSourceRoute", new WmiValue(false)),
        ("ArpUseEtherSNAP", new WmiValue(false)),
        ("RegistryKeyByDNS", new WmiValue(false)),
        ("TcpipNetbiosOptions", new WmiValue(false)),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem"))
    );

    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> IPDisabledRow() => WmiRow.Build(
        ("IPEnabled", new WmiValue(false)),
        ("MACAddress", new WmiValue("00:00:00:00:00:00")),
        ("Caption", new WmiValue("WAN Miniport"))
    );

    [Fact]
    public async Task IPEnabled_Adapter_Is_Included()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapterConfiguration", new[] { IPEnabledRow() });
        var results = await provider.ToSafeNetworkAdapterConfigMetricsAsync(CancellationToken.None);

        Assert.Single(results);
    }

    [Fact]
    public async Task IPDisabled_Adapter_Is_Filtered_Out()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapterConfiguration",
            new[] { IPDisabledRow() });
        var results = await provider.ToSafeNetworkAdapterConfigMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task Mixed_Rows_Only_IPEnabled_Returned()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapterConfiguration",
            new[] { IPDisabledRow(), IPEnabledRow("11:22:33:44:55:66"), IPDisabledRow() });
        var results = await provider.ToSafeNetworkAdapterConfigMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("11:22:33:44:55:66", results[0].MACAddress);
    }

    [Fact]
    public async Task FullData_Maps_DHCPEnabled()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapterConfiguration", new[] { IPEnabledRow() });
        var results = await provider.ToSafeNetworkAdapterConfigMetricsAsync(CancellationToken.None);

        Assert.True(results[0].DHCPEnabled);
    }

    [Fact]
    public async Task FullData_Maps_IPEnabled_True()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapterConfiguration", new[] { IPEnabledRow() });
        var results = await provider.ToSafeNetworkAdapterConfigMetricsAsync(CancellationToken.None);

        Assert.True(results[0].IPEnabled);
    }

    [Fact]
    public async Task FullData_Maps_DNSHostName()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapterConfiguration", new[] { IPEnabledRow() });
        var results = await provider.ToSafeNetworkAdapterConfigMetricsAsync(CancellationToken.None);

        Assert.Equal("DESKTOP-01", results[0].DNSHostName);
    }

    [Fact]
    public async Task FullData_Maps_DNSServerSearchOrder_Flattened()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapterConfiguration", new[] { IPEnabledRow() });
        var results = await provider.ToSafeNetworkAdapterConfigMetricsAsync(CancellationToken.None);

        Assert.Equal("8.8.8.8, 8.8.4.4", results[0].DNSServerSearchOrder);
    }

    [Fact]
    public async Task FullData_Maps_DefaultIPGateway_Flattened()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapterConfiguration", new[] { IPEnabledRow() });
        var results = await provider.ToSafeNetworkAdapterConfigMetricsAsync(CancellationToken.None);

        Assert.Equal("192.168.1.1", results[0].DefaultIPGateway);
    }

    [Fact]
    public async Task FullData_Maps_IPAddress_Flattened()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapterConfiguration", new[] { IPEnabledRow() });
        var results = await provider.ToSafeNetworkAdapterConfigMetricsAsync(CancellationToken.None);

        Assert.Equal("192.168.1.100, ::1", results[0].IPAddress);
    }

    [Fact]
    public async Task FullData_Maps_IPSubnet_Flattened()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapterConfiguration", new[] { IPEnabledRow() });
        var results = await provider.ToSafeNetworkAdapterConfigMetricsAsync(CancellationToken.None);

        Assert.Equal("255.255.255.0, 128", results[0].IPSubnet);
    }

    [Fact]
    public async Task FullData_Maps_MTU_Uint()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapterConfiguration", new[] { IPEnabledRow() });
        var results = await provider.ToSafeNetworkAdapterConfigMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)1500, results[0].MTU);
    }

    [Fact]
    public async Task FullData_Maps_Index_Uint()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapterConfiguration", new[] { IPEnabledRow() });
        var results = await provider.ToSafeNetworkAdapterConfigMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)1, results[0].Index);
    }

    [Fact]
    public async Task FullData_Maps_DefaultTTL_Uint()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapterConfiguration", new[] { IPEnabledRow() });
        var results = await provider.ToSafeNetworkAdapterConfigMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)128, results[0].DefaultTTL);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapterConfiguration", WmiRow.Empty());
        var results = await provider.ToSafeNetworkAdapterConfigMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task Missing_IPEnabled_Key_Defaults_To_False_And_Filtered_Out()
    {
        // No IPEnabled key → GetBool returns null → ?? false → filtered out
        var row = WmiRow.Build(("MACAddress", new WmiValue("AA:BB:CC:DD:EE:FF")));
        var provider = new FakeWmiProvider("Win32_NetworkAdapterConfiguration", new[] { row });
        var results = await provider.ToSafeNetworkAdapterConfigMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }
}
