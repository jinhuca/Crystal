using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.SoftwareFeatures.Share;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.SoftwareFeatures;

public class ShareExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> FullRow() => WmiRow.Build(
        ("AllowMaximum", new WmiValue(true)),
        ("Caption", new WmiValue("Remote Admin")),
        ("Description", new WmiValue("Remote Admin")),
        ("InstallDate", new WmiValue(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("MaximumAllowed", new WmiValue(0)),
        ("Name", new WmiValue("ADMIN$")),
        ("Path", new WmiValue("C:\\Windows")),
        ("Status", new WmiValue("OK")),
        ("Type", new WmiValue(0))
    );

    [Fact]
    public async Task FullData_Maps_Name_And_Path()
    {
        var provider = new FakeWmiProvider("Win32_Share", new[] { FullRow() });
        var results = await provider.ToSafeShareMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("ADMIN$", results[0].Name);
        Assert.Equal("C:\\Windows", results[0].Path);
    }

    [Fact]
    public async Task FullData_Maps_AllowMaximum_True()
    {
        var provider = new FakeWmiProvider("Win32_Share", new[] { FullRow() });
        var results = await provider.ToSafeShareMetricsAsync(CancellationToken.None);

        Assert.True(results[0].AllowMaximum);
    }

    [Fact]
    public async Task FullData_Maps_Type_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Share", new[] { FullRow() });
        var results = await provider.ToSafeShareMetricsAsync(CancellationToken.None);

        Assert.Equal(0u, results[0].Type);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_Share", WmiRow.Empty());
        var results = await provider.ToSafeShareMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeShareMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("public")));

        var provider = new FakeWmiProvider("Win32_Share", new[] { partial });
        var results = await provider.ToSafeShareMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("public", results[0].Name);
        Assert.Null(results[0].Path);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // Path stored as a bool instead of String — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("Path", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_Share", new[] { badRow });
        var results = await provider.ToSafeShareMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Path);
    }

    [Fact]
    public async Task MultipleShares_Returns_All()
    {
        var a = WmiRow.Build(("Name", new WmiValue("ADMIN$")));
        var b = WmiRow.Build(("Name", new WmiValue("public")));

        var provider = new FakeWmiProvider("Win32_Share", new[] { a, b });
        var results = await provider.ToSafeShareMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("ADMIN$", results[0].Name);
        Assert.Equal("public", results[1].Name);
    }
}
