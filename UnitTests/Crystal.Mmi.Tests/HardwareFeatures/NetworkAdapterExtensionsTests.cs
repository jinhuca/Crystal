using Crystal.Mmi.HardwareFeatures.NetworkAdapter;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class NetworkAdapterExtensionsTests
{
    /// <summary>Builds a row that passes the physical + MAC filter.</summary>
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> PhysicalAdapterRow(
        string name = "Intel Ethernet", string mac = "AA:BB:CC:DD:EE:FF") => WmiRow.Build(
        ("PhysicalAdapter", new WmiValue(true)),
        ("MACAddress", new WmiValue(mac)),
        ("Name", new WmiValue(name)),
        ("Description", new WmiValue(name)),
        ("Manufacturer", new WmiValue("Intel")),
        ("DeviceID", new WmiValue("1")),
        ("NetConnectionID", new WmiValue("Ethernet")),
        ("NetConnectionStatus", new WmiValue(2)),
        ("NetEnabled", new WmiValue(true)),
        ("Speed", new WmiValue(1_000_000_000UL)),
        ("Index", new WmiValue(1)),
        ("Availability", new WmiValue(3)),
        ("Status", new WmiValue("OK")),
        ("ServiceName", new WmiValue("e1i65x64")),
        ("GUID", new WmiValue("{AAAA-BBBB}")),
        ("ProductName", new WmiValue("Intel Ethernet")),
        ("PowerManagementSupported", new WmiValue(false)),
        ("Installed", new WmiValue(true)),
        ("ConfigManagerUserConfig", new WmiValue(false))
    );

    /// <summary>Builds a software/virtual adapter that should be filtered out.</summary>
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> VirtualAdapterRow() => WmiRow.Build(
        ("PhysicalAdapter", new WmiValue(false)),
        ("MACAddress", new WmiValue("00:00:00:00:00:00")),
        ("Name", new WmiValue("WAN Miniport (IP)")),
        ("NetConnectionStatus", new WmiValue(0))
    );

    /// <summary>Physical adapter but no MAC address — should be filtered out.</summary>
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> PhysicalNoMacRow() => WmiRow.Build(
        ("PhysicalAdapter", new WmiValue(true)),
        ("Name", new WmiValue("Mystery NIC"))
        // no MACAddress key
    );

    [Fact]
    public async Task PhysicalWithMac_Is_Included()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapter", new[] { PhysicalAdapterRow() });
        var results = await provider.ToSafeNetworkAdapterMetricsAsync(CancellationToken.None);

        Assert.Single(results);
    }

    [Fact]
    public async Task VirtualAdapter_Is_Filtered_Out()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapter", new[] { VirtualAdapterRow() });
        var results = await provider.ToSafeNetworkAdapterMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task PhysicalWithNoMac_Is_Filtered_Out()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapter", new[] { PhysicalNoMacRow() });
        var results = await provider.ToSafeNetworkAdapterMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task Mixed_Adapters_Only_Physical_With_MAC_Returned()
    {
        var rows = new[]
        {
            VirtualAdapterRow(),
            PhysicalAdapterRow("Real NIC", "11:22:33:44:55:66"),
            PhysicalNoMacRow()
        };
        var provider = new FakeWmiProvider("Win32_NetworkAdapter", rows);
        var results = await provider.ToSafeNetworkAdapterMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Real NIC", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapter", new[] { PhysicalAdapterRow() });
        var results = await provider.ToSafeNetworkAdapterMetricsAsync(CancellationToken.None);

        Assert.Equal("Intel Ethernet", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_MACAddress()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapter", new[] { PhysicalAdapterRow() });
        var results = await provider.ToSafeNetworkAdapterMetricsAsync(CancellationToken.None);

        Assert.Equal("AA:BB:CC:DD:EE:FF", results[0].MACAddress);
    }

    [Fact]
    public async Task FullData_Maps_Speed_ULong()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapter", new[] { PhysicalAdapterRow() });
        var results = await provider.ToSafeNetworkAdapterMetricsAsync(CancellationToken.None);

        Assert.Equal(1_000_000_000UL, results[0].Speed);
    }

    [Fact]
    public async Task FullData_Maps_NetConnectionStatus_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapter", new[] { PhysicalAdapterRow() });
        var results = await provider.ToSafeNetworkAdapterMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)2, results[0].NetConnectionStatus);
    }

    [Fact]
    public async Task FullData_Maps_NetEnabled_True()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapter", new[] { PhysicalAdapterRow() });
        var results = await provider.ToSafeNetworkAdapterMetricsAsync(CancellationToken.None);

        Assert.True(results[0].NetEnabled);
    }

    [Fact]
    public async Task FullData_Maps_PhysicalAdapter_True()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapter", new[] { PhysicalAdapterRow() });
        var results = await provider.ToSafeNetworkAdapterMetricsAsync(CancellationToken.None);

        Assert.True(results[0].PhysicalAdapter);
    }

    [Fact]
    public async Task FullData_Maps_Manufacturer()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapter", new[] { PhysicalAdapterRow() });
        var results = await provider.ToSafeNetworkAdapterMetricsAsync(CancellationToken.None);

        Assert.Equal("Intel", results[0].Manufacturer);
    }

    [Fact]
    public async Task FullData_Maps_Availability_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapter", new[] { PhysicalAdapterRow() });
        var results = await provider.ToSafeNetworkAdapterMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].Availability);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_NetworkAdapter", WmiRow.Empty());
        var results = await provider.ToSafeNetworkAdapterMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task Multiple_Physical_Adapters_Returns_All()
    {
        var nic1 = PhysicalAdapterRow("NIC1", "AA:BB:CC:DD:EE:01");
        var nic2 = PhysicalAdapterRow("NIC2", "AA:BB:CC:DD:EE:02");
        var provider = new FakeWmiProvider("Win32_NetworkAdapter", new[] { nic1, nic2 });
        var results = await provider.ToSafeNetworkAdapterMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("NIC1", results[0].Name);
        Assert.Equal("NIC2", results[1].Name);
    }

    // ── ConnectionStatePhrase ───────────────────────────────────────────────

    [Theory]
    [InlineData(0, "Disconnected")]
    [InlineData(1, "Connecting...")]
    [InlineData(2, "Connected (Active)")]
    [InlineData(3, "Disconnecting...")]
    [InlineData(7, "Hardware Not Present")]
    [InlineData(11, "Authentication Failed")]
    [InlineData(99, "Idle / Sleeping")]
    public void ConnectionStatePhrase_Maps_Correctly(int code, string expected)
    {
        // Availability=null, Caption=null, ... NetConnectionStatus at position 21, PhysicalAdapter=true at 28
        // 36 constructor args total for NetworkAdapterMetrics
        var m = new NetworkAdapterMetrics(
            null, null, null, null, null, null, null, null, null, null,  // 0-9
            null, null, null, null, null, null, null, null, null, null,  // 10-19
            null, (ushort)code, null, null, null, null, null, null, true, // 20-28
            null, null, null, null, null, null, null);                    // 29-35

        Assert.Equal(expected, m.ConnectionStatePhrase);
    }

    [Fact]
    public void ConnectionStatePhrase_Null_Status_Returns_Idle()
    {
        var m = new NetworkAdapterMetrics(
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, true,
            null, null, null, null, null, null, null);

        Assert.Equal("Idle / Sleeping", m.ConnectionStatePhrase);
    }

    // ── FormattedLinkSpeed ─────────────────────────────────────────────────

    [Fact]
    public void FormattedLinkSpeed_Null_Returns_Unknown()
        => Assert.Equal("Unknown / Link Down", MakeAdapterWithSpeed(null).FormattedLinkSpeed);

    [Fact]
    public void FormattedLinkSpeed_Zero_Returns_Unknown()
        => Assert.Equal("Unknown / Link Down", MakeAdapterWithSpeed(0).FormattedLinkSpeed);

    [Fact]
    public void FormattedLinkSpeed_100Mbps()
        => Assert.Equal("100 Mbps", MakeAdapterWithSpeed(100_000_000).FormattedLinkSpeed);

    [Fact]
    public void FormattedLinkSpeed_1Gbps()
        => Assert.Equal("1.0 Gbps", MakeAdapterWithSpeed(1_000_000_000).FormattedLinkSpeed);

    [Fact]
    public void FormattedLinkSpeed_10Gbps()
        => Assert.Equal("10.0 Gbps", MakeAdapterWithSpeed(10_000_000_000).FormattedLinkSpeed);

    [Fact]
    public void FormattedLinkSpeed_500Mbps()
        => Assert.Equal("500 Mbps", MakeAdapterWithSpeed(500_000_000).FormattedLinkSpeed);

    // Speed is at position 30, PhysicalAdapter=true at position 28
    // 36 total constructor args
    private static NetworkAdapterMetrics MakeAdapterWithSpeed(ulong? speed)
        => new NetworkAdapterMetrics(
            null, null, null, null, null, null, null, null, null, null, // 0-9
            null, null, null, null, null, null, null, null, null, null, // 10-19
            null, 2, null, null, null, null, null, null, true,           // 20-28
            null, speed, null, null, null, null, null);                  // 29-35
}
