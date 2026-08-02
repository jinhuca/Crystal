using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.SoftwareFeatures.Environment;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.SoftwareFeatures;

public class EnvironmentExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> PathVarRow() => WmiRow.Build(
        ("Caption", new WmiValue("<System>\\Path")),
        ("Description", new WmiValue("<System>\\Path")),
        ("InstallDate", new WmiValue(new DateTime(2021, 2, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("Name", new WmiValue("Path")),
        ("Status", new WmiValue("OK")),
        ("SystemVariable", new WmiValue(true)),
        ("UserName", new WmiValue("<System>")),
        ("VariableValue", new WmiValue("C:\\Windows\\system32;C:\\Windows"))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_Environment", new[] { PathVarRow() });
        var results = await provider.ToSafeEnvironmentMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Path", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_VariableValue()
    {
        var provider = new FakeWmiProvider("Win32_Environment", new[] { PathVarRow() });
        var results = await provider.ToSafeEnvironmentMetricsAsync(CancellationToken.None);

        Assert.Equal("C:\\Windows\\system32;C:\\Windows", results[0].VariableValue);
    }

    [Fact]
    public async Task FullData_Maps_SystemVariable_True()
    {
        var provider = new FakeWmiProvider("Win32_Environment", new[] { PathVarRow() });
        var results = await provider.ToSafeEnvironmentMetricsAsync(CancellationToken.None);

        Assert.True(results[0].SystemVariable);
    }

    [Fact]
    public async Task FullData_Maps_UserName()
    {
        var provider = new FakeWmiProvider("Win32_Environment", new[] { PathVarRow() });
        var results = await provider.ToSafeEnvironmentMetricsAsync(CancellationToken.None);

        Assert.Equal("<System>", results[0].UserName);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_Environment", new[] { PathVarRow() });
        var results = await provider.ToSafeEnvironmentMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_Environment", new[] { PathVarRow() });
        var results = await provider.ToSafeEnvironmentMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2021, 2, 1, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_Environment", WmiRow.Empty());
        var results = await provider.ToSafeEnvironmentMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeEnvironmentMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleVariables_Returns_All()
    {
        var var1 = WmiRow.Build(("Name", new WmiValue("Path")));
        var var2 = WmiRow.Build(("Name", new WmiValue("TEMP")));

        var provider = new FakeWmiProvider("Win32_Environment", new[] { var1, var2 });
        var results = await provider.ToSafeEnvironmentMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Path", results[0].Name);
        Assert.Equal("TEMP", results[1].Name);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Variable Without Value")));

        var provider = new FakeWmiProvider("Win32_Environment", new[] { partial });
        var results = await provider.ToSafeEnvironmentMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Variable Without Value", results[0].Name);
        Assert.Null(results[0].VariableValue);
        Assert.Null(results[0].SystemVariable);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // SystemVariable stored as a string instead of Bool — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("SystemVariable", new WmiValue("true")));

        var provider = new FakeWmiProvider("Win32_Environment", new[] { badRow });
        var results = await provider.ToSafeEnvironmentMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].SystemVariable);
    }
}
