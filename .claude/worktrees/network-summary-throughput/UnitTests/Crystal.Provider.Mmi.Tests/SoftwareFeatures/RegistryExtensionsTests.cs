using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.SoftwareFeatures.Registry;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.SoftwareFeatures;

public class RegistryExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> RegistryRow() => WmiRow.Build(
        ("Caption", new WmiValue("Microsoft Windows XP Professional|C:\\WINDOWS|\\Device\\Harddisk0\\Partition1")),
        ("CurrentSize", new WmiValue(10)),
        ("Description", new WmiValue("Microsoft Windows XP Professional|C:\\WINDOWS|\\Device\\Harddisk0\\Partition1")),
        ("InstallDate", new WmiValue(new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("MaximumSize", new WmiValue(786432)),
        ("Name", new WmiValue("Microsoft Windows XP Professional|C:\\WINDOWS|\\Device\\Harddisk0\\Partition1")),
        ("ProposedSize", new WmiValue(786432)),
        ("Status", new WmiValue("OK"))
    );

    [Fact]
    public async Task FullData_Maps_CurrentSize_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Registry", new[] { RegistryRow() });
        var results = await provider.ToSafeRegistryMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal(10u, results[0].CurrentSize);
    }

    [Fact]
    public async Task FullData_Maps_MaximumSize_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Registry", new[] { RegistryRow() });
        var results = await provider.ToSafeRegistryMetricsAsync(CancellationToken.None);

        Assert.Equal(786432u, results[0].MaximumSize);
    }

    [Fact]
    public async Task FullData_Maps_ProposedSize_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Registry", new[] { RegistryRow() });
        var results = await provider.ToSafeRegistryMetricsAsync(CancellationToken.None);

        Assert.Equal(786432u, results[0].ProposedSize);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_Registry", new[] { RegistryRow() });
        var results = await provider.ToSafeRegistryMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_Registry", new[] { RegistryRow() });
        var results = await provider.ToSafeRegistryMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_Registry", WmiRow.Empty());
        var results = await provider.ToSafeRegistryMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeRegistryMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Registry Without Sizes")));

        var provider = new FakeWmiProvider("Win32_Registry", new[] { partial });
        var results = await provider.ToSafeRegistryMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Registry Without Sizes", results[0].Name);
        Assert.Null(results[0].CurrentSize);
        Assert.Null(results[0].MaximumSize);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // CurrentSize stored as a string instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("CurrentSize", new WmiValue("10")));

        var provider = new FakeWmiProvider("Win32_Registry", new[] { badRow });
        var results = await provider.ToSafeRegistryMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].CurrentSize);
    }
}
