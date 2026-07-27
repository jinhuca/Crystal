using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.SoftwareFeatures.Group;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.SoftwareFeatures;

public class GroupExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> FullRow() => WmiRow.Build(
        ("Caption", new WmiValue("DESKTOP-01\\Administrators")),
        ("Description", new WmiValue("Administrators have complete and unrestricted access")),
        ("Domain", new WmiValue("DESKTOP-01")),
        ("InstallDate", new WmiValue(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("LocalAccount", new WmiValue(true)),
        ("Name", new WmiValue("Administrators")),
        ("SID", new WmiValue("S-1-5-32-544")),
        ("SIDType", new WmiValue(4)),
        ("Status", new WmiValue("OK"))
    );

    [Fact]
    public async Task FullData_Maps_Name_And_Domain()
    {
        var provider = new FakeWmiProvider("Win32_Group", new[] { FullRow() });
        var results = await provider.ToSafeGroupMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Administrators", results[0].Name);
        Assert.Equal("DESKTOP-01", results[0].Domain);
    }

    [Fact]
    public async Task FullData_Maps_SID()
    {
        var provider = new FakeWmiProvider("Win32_Group", new[] { FullRow() });
        var results = await provider.ToSafeGroupMetricsAsync(CancellationToken.None);

        Assert.Equal("S-1-5-32-544", results[0].SID);
    }

    [Fact]
    public async Task FullData_Maps_SIDType_Byte()
    {
        var provider = new FakeWmiProvider("Win32_Group", new[] { FullRow() });
        var results = await provider.ToSafeGroupMetricsAsync(CancellationToken.None);

        Assert.Equal((byte)4, results[0].SIDType);
    }

    [Fact]
    public async Task FullData_Maps_LocalAccount_True()
    {
        var provider = new FakeWmiProvider("Win32_Group", new[] { FullRow() });
        var results = await provider.ToSafeGroupMetricsAsync(CancellationToken.None);

        Assert.True(results[0].LocalAccount);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_Group", WmiRow.Empty());
        var results = await provider.ToSafeGroupMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeGroupMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Users")));

        var provider = new FakeWmiProvider("Win32_Group", new[] { partial });
        var results = await provider.ToSafeGroupMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Users", results[0].Name);
        Assert.Null(results[0].SID);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // SID stored as a bool instead of String — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("SID", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_Group", new[] { badRow });
        var results = await provider.ToSafeGroupMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].SID);
    }

    [Fact]
    public async Task MultipleGroups_Returns_All()
    {
        var a = WmiRow.Build(("Name", new WmiValue("Administrators")));
        var b = WmiRow.Build(("Name", new WmiValue("Users")));

        var provider = new FakeWmiProvider("Win32_Group", new[] { a, b });
        var results = await provider.ToSafeGroupMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Administrators", results[0].Name);
        Assert.Equal("Users", results[1].Name);
    }
}
