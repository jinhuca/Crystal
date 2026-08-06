using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.SoftwareFeatures.NetworkConnection;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.SoftwareFeatures;

public class NetworkConnectionExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> MappedDriveRow() => WmiRow.Build(
        ("AccessMask", new WmiValue(1179785)),
        ("Caption", new WmiValue("\\\\NTRELEASE (Z:)")),
        ("Comment", new WmiValue("")),
        ("ConnectionState", new WmiValue("Connected")),
        ("ConnectionType", new WmiValue("Persistent Connection")),
        ("Description", new WmiValue("\\\\NTRELEASE (Z:)")),
        ("DisplayType", new WmiValue("Share")),
        ("InstallDate", new WmiValue(new DateTime(2023, 9, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("LocalName", new WmiValue("Z:")),
        ("Name", new WmiValue("\\\\NTRELEASE (Z:)")),
        ("Persistent", new WmiValue(true)),
        ("ProviderName", new WmiValue("Microsoft Windows Network")),
        ("RemoteName", new WmiValue("\\\\NTRELEASE")),
        ("RemotePath", new WmiValue("\\\\NTRELEASE\\Public")),
        ("ResourceType", new WmiValue("Disk")),
        ("Status", new WmiValue("OK")),
        ("UserName", new WmiValue("SYSTEM"))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_NetworkConnection", new[] { MappedDriveRow() });
        var results = await provider.ToSafeNetworkConnectionMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("\\\\NTRELEASE (Z:)", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_LocalName()
    {
        var provider = new FakeWmiProvider("Win32_NetworkConnection", new[] { MappedDriveRow() });
        var results = await provider.ToSafeNetworkConnectionMetricsAsync(CancellationToken.None);

        Assert.Equal("Z:", results[0].LocalName);
    }

    [Fact]
    public async Task FullData_Maps_RemoteName()
    {
        var provider = new FakeWmiProvider("Win32_NetworkConnection", new[] { MappedDriveRow() });
        var results = await provider.ToSafeNetworkConnectionMetricsAsync(CancellationToken.None);

        Assert.Equal("\\\\NTRELEASE", results[0].RemoteName);
    }

    [Fact]
    public async Task FullData_Maps_ConnectionState()
    {
        var provider = new FakeWmiProvider("Win32_NetworkConnection", new[] { MappedDriveRow() });
        var results = await provider.ToSafeNetworkConnectionMetricsAsync(CancellationToken.None);

        Assert.Equal("Connected", results[0].ConnectionState);
    }

    [Fact]
    public async Task FullData_Maps_Persistent_True()
    {
        var provider = new FakeWmiProvider("Win32_NetworkConnection", new[] { MappedDriveRow() });
        var results = await provider.ToSafeNetworkConnectionMetricsAsync(CancellationToken.None);

        Assert.True(results[0].Persistent);
    }

    [Fact]
    public async Task FullData_Maps_AccessMask_Uint()
    {
        var provider = new FakeWmiProvider("Win32_NetworkConnection", new[] { MappedDriveRow() });
        var results = await provider.ToSafeNetworkConnectionMetricsAsync(CancellationToken.None);

        Assert.Equal(1179785u, results[0].AccessMask);
    }

    [Fact]
    public async Task FullData_Maps_ResourceType()
    {
        var provider = new FakeWmiProvider("Win32_NetworkConnection", new[] { MappedDriveRow() });
        var results = await provider.ToSafeNetworkConnectionMetricsAsync(CancellationToken.None);

        Assert.Equal("Disk", results[0].ResourceType);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_NetworkConnection", new[] { MappedDriveRow() });
        var results = await provider.ToSafeNetworkConnectionMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2023, 9, 1, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_NetworkConnection", WmiRow.Empty());
        var results = await provider.ToSafeNetworkConnectionMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeNetworkConnectionMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleConnections_Returns_All()
    {
        var conn1 = WmiRow.Build(("LocalName", new WmiValue("Z:")));
        var conn2 = WmiRow.Build(("LocalName", new WmiValue("Y:")));

        var provider = new FakeWmiProvider("Win32_NetworkConnection", new[] { conn1, conn2 });
        var results = await provider.ToSafeNetworkConnectionMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Z:", results[0].LocalName);
        Assert.Equal("Y:", results[1].LocalName);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("LocalName", new WmiValue("Z:")));

        var provider = new FakeWmiProvider("Win32_NetworkConnection", new[] { partial });
        var results = await provider.ToSafeNetworkConnectionMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Z:", results[0].LocalName);
        Assert.Null(results[0].RemoteName);
        Assert.Null(results[0].Persistent);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // Persistent stored as a string instead of Bool — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("Persistent", new WmiValue("true")));

        var provider = new FakeWmiProvider("Win32_NetworkConnection", new[] { badRow });
        var results = await provider.ToSafeNetworkConnectionMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Persistent);
    }
}
