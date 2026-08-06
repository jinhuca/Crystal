using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.SoftwareFeatures.NetworkProtocol;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.SoftwareFeatures;

public class NetworkProtocolExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> TcpIpRow() => WmiRow.Build(
        ("Caption", new WmiValue("TCP/IP")),
        ("ConnectionlessService", new WmiValue(true)),
        ("Description", new WmiValue("TCP/IP")),
        ("GuaranteesDelivery", new WmiValue(false)),
        ("GuaranteesSequencing", new WmiValue(false)),
        ("InstallDate", new WmiValue(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("MaximumAddressSize", new WmiValue(16)),
        ("MaximumMessageSize", new WmiValue(0)),
        ("MessageOriented", new WmiValue(false)),
        ("MinimumAddressSize", new WmiValue(16)),
        ("Name", new WmiValue("TCP/IP")),
        ("PseudoStreamOriented", new WmiValue(false)),
        ("Status", new WmiValue("OK")),
        ("SupportsBroadcasting", new WmiValue(true)),
        ("SupportsConnectData", new WmiValue(false)),
        ("SupportsDisconnectData", new WmiValue(false)),
        ("SupportsEncryption", new WmiValue(false)),
        ("SupportsExpeditedData", new WmiValue(false)),
        ("SupportsFragmentation", new WmiValue(true)),
        ("SupportsGracefulClosing", new WmiValue(true)),
        ("SupportsGuaranteedBandwidth", new WmiValue(false)),
        ("SupportsMulticasting", new WmiValue(true)),
        ("SupportsQualityofService", new WmiValue(false))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_NetworkProtocol", new[] { TcpIpRow() });
        var results = await provider.ToSafeNetworkProtocolMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("TCP/IP", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_ConnectionlessService_True()
    {
        var provider = new FakeWmiProvider("Win32_NetworkProtocol", new[] { TcpIpRow() });
        var results = await provider.ToSafeNetworkProtocolMetricsAsync(CancellationToken.None);

        Assert.True(results[0].ConnectionlessService);
    }

    [Fact]
    public async Task FullData_Maps_MaximumAddressSize_Uint()
    {
        var provider = new FakeWmiProvider("Win32_NetworkProtocol", new[] { TcpIpRow() });
        var results = await provider.ToSafeNetworkProtocolMetricsAsync(CancellationToken.None);

        Assert.Equal(16u, results[0].MaximumAddressSize);
    }

    [Fact]
    public async Task FullData_Maps_SupportsMulticasting_True()
    {
        var provider = new FakeWmiProvider("Win32_NetworkProtocol", new[] { TcpIpRow() });
        var results = await provider.ToSafeNetworkProtocolMetricsAsync(CancellationToken.None);

        Assert.True(results[0].SupportsMulticasting);
    }

    [Fact]
    public async Task FullData_Maps_SupportsQualityofService_False()
    {
        var provider = new FakeWmiProvider("Win32_NetworkProtocol", new[] { TcpIpRow() });
        var results = await provider.ToSafeNetworkProtocolMetricsAsync(CancellationToken.None);

        Assert.False(results[0].SupportsQualityofService);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_NetworkProtocol", new[] { TcpIpRow() });
        var results = await provider.ToSafeNetworkProtocolMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_NetworkProtocol", new[] { TcpIpRow() });
        var results = await provider.ToSafeNetworkProtocolMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_NetworkProtocol", WmiRow.Empty());
        var results = await provider.ToSafeNetworkProtocolMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeNetworkProtocolMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleProtocols_Returns_All()
    {
        var tcp = WmiRow.Build(("Name", new WmiValue("TCP/IP")));
        var udp = WmiRow.Build(("Name", new WmiValue("UDP")));

        var provider = new FakeWmiProvider("Win32_NetworkProtocol", new[] { tcp, udp });
        var results = await provider.ToSafeNetworkProtocolMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("TCP/IP", results[0].Name);
        Assert.Equal("UDP", results[1].Name);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Protocol Without Flags")));

        var provider = new FakeWmiProvider("Win32_NetworkProtocol", new[] { partial });
        var results = await provider.ToSafeNetworkProtocolMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Protocol Without Flags", results[0].Name);
        Assert.Null(results[0].SupportsMulticasting);
        Assert.Null(results[0].MaximumAddressSize);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // MaximumAddressSize stored as a bool instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("MaximumAddressSize", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_NetworkProtocol", new[] { badRow });
        var results = await provider.ToSafeNetworkProtocolMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].MaximumAddressSize);
    }
}
