using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.SoftwareFeatures.QuickFixEngineering;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.SoftwareFeatures;

public class QuickFixEngineeringExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> FullRow() => WmiRow.Build(
        ("Caption", new WmiValue("https://support.microsoft.com/kb/4533002")),
        ("CSName", new WmiValue("DESKTOP-01")),
        ("Description", new WmiValue("Update")),
        ("FixComments", new WmiValue("")),
        ("HotFixID", new WmiValue("KB4533002")),
        ("InstallDate", new WmiValue(new DateTime(2020, 2, 11, 0, 0, 0, DateTimeKind.Utc))),
        ("InstalledBy", new WmiValue("NT AUTHORITY\\SYSTEM")),
        ("InstalledOn", new WmiValue("2/11/2020")),
        ("Name", new WmiValue("")),
        ("ServicePackInEffect", new WmiValue("")),
        ("Status", new WmiValue("")));

    [Fact]
    public async Task FullData_Maps_HotFixID()
    {
        var provider = new FakeWmiProvider("Win32_QuickFixEngineering", new[] { FullRow() });
        var results = await provider.ToSafeQuickFixEngineeringMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("KB4533002", results[0].HotFixID);
    }

    [Fact]
    public async Task FullData_Maps_InstalledBy_And_InstalledOn()
    {
        var provider = new FakeWmiProvider("Win32_QuickFixEngineering", new[] { FullRow() });
        var results = await provider.ToSafeQuickFixEngineeringMetricsAsync(CancellationToken.None);

        Assert.Equal("NT AUTHORITY\\SYSTEM", results[0].InstalledBy);
        Assert.Equal("2/11/2020", results[0].InstalledOn);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_QuickFixEngineering", new[] { FullRow() });
        var results = await provider.ToSafeQuickFixEngineeringMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2020, 2, 11, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_QuickFixEngineering", WmiRow.Empty());
        var results = await provider.ToSafeQuickFixEngineeringMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeQuickFixEngineeringMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("HotFixID", new WmiValue("KB5001330")));

        var provider = new FakeWmiProvider("Win32_QuickFixEngineering", new[] { partial });
        var results = await provider.ToSafeQuickFixEngineeringMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("KB5001330", results[0].HotFixID);
        Assert.Null(results[0].InstalledBy);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // InstallDate stored as a String instead of DateTime — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("InstallDate", new WmiValue("not-a-date")));

        var provider = new FakeWmiProvider("Win32_QuickFixEngineering", new[] { badRow });
        var results = await provider.ToSafeQuickFixEngineeringMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].InstallDate);
    }

    [Fact]
    public async Task MultipleHotfixes_Returns_All()
    {
        var a = WmiRow.Build(("HotFixID", new WmiValue("KB4533002")));
        var b = WmiRow.Build(("HotFixID", new WmiValue("KB5001330")));

        var provider = new FakeWmiProvider("Win32_QuickFixEngineering", new[] { a, b });
        var results = await provider.ToSafeQuickFixEngineeringMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("KB4533002", results[0].HotFixID);
        Assert.Equal("KB5001330", results[1].HotFixID);
    }
}
