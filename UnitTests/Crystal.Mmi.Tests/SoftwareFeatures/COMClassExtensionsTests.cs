using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.SoftwareFeatures.COMClass;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.SoftwareFeatures;

public class COMClassExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> FullRow() => WmiRow.Build(
        ("Caption", new WmiValue("Shell Automation Service")),
        ("Description", new WmiValue("Shell Automation Service")),
        ("InstallDate", new WmiValue(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("Name", new WmiValue("Shell Automation Service")),
        ("Status", new WmiValue("OK")));

    [Fact]
    public async Task FullData_Maps_Name_And_Caption()
    {
        var provider = new FakeWmiProvider("Win32_COMClass", new[] { FullRow() });
        var results = await provider.ToSafeCOMClassMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Shell Automation Service", results[0].Name);
        Assert.Equal("Shell Automation Service", results[0].Caption);
    }

    [Fact]
    public async Task FullData_Maps_Status()
    {
        var provider = new FakeWmiProvider("Win32_COMClass", new[] { FullRow() });
        var results = await provider.ToSafeCOMClassMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_COMClass", WmiRow.Empty());
        var results = await provider.ToSafeCOMClassMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeCOMClassMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Some COM Component")));

        var provider = new FakeWmiProvider("Win32_COMClass", new[] { partial });
        var results = await provider.ToSafeCOMClassMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Some COM Component", results[0].Name);
        Assert.Null(results[0].Status);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // Name stored as a bool instead of String — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("Name", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_COMClass", new[] { badRow });
        var results = await provider.ToSafeCOMClassMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Name);
    }

    [Fact]
    public async Task MultipleComponents_Returns_All()
    {
        var a = WmiRow.Build(("Name", new WmiValue("Shell Automation Service")));
        var b = WmiRow.Build(("Name", new WmiValue("Task Scheduler")));

        var provider = new FakeWmiProvider("Win32_COMClass", new[] { a, b });
        var results = await provider.ToSafeCOMClassMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Shell Automation Service", results[0].Name);
        Assert.Equal("Task Scheduler", results[1].Name);
    }
}
