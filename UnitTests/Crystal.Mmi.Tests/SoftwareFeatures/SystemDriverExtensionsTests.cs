using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.SoftwareFeatures.SystemDriver;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.SoftwareFeatures;

public class SystemDriverExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> DriverRow() => WmiRow.Build(
        ("AcceptPause", new WmiValue(false)),
        ("AcceptStop", new WmiValue(true)),
        ("Caption", new WmiValue("LXSS Manager")),
        ("CreationClassName", new WmiValue("Win32_SystemDriver")),
        ("Description", new WmiValue("LXSS Manager")),
        ("DesktopInteract", new WmiValue(false)),
        ("DisplayName", new WmiValue("LXSS Manager")),
        ("ErrorControl", new WmiValue("Normal")),
        ("ExitCode", new WmiValue(0)),
        ("InstallationDate", new WmiValue(new DateTime(2021, 5, 5, 0, 0, 0, DateTimeKind.Utc))),
        ("Name", new WmiValue("LxssManager")),
        ("PathName", new WmiValue("C:\\Windows\\system32\\svchost.exe -k LocalServiceNetworkRestricted")),
        ("ServiceSpecificExitCode", new WmiValue(0)),
        ("ServiceType", new WmiValue("Share Process")),
        ("Started", new WmiValue(true)),
        ("StartMode", new WmiValue("Auto")),
        ("StartName", new WmiValue("LocalSystem")),
        ("State", new WmiValue("Running")),
        ("Status", new WmiValue("OK")),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("TagId", new WmiValue(24))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_SystemDriver", new[] { DriverRow() });
        var results = await provider.ToSafeSystemDriverMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("LxssManager", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_PathName()
    {
        var provider = new FakeWmiProvider("Win32_SystemDriver", new[] { DriverRow() });
        var results = await provider.ToSafeSystemDriverMetricsAsync(CancellationToken.None);

        Assert.Equal("C:\\Windows\\system32\\svchost.exe -k LocalServiceNetworkRestricted", results[0].PathName);
    }

    [Fact]
    public async Task FullData_Maps_State()
    {
        var provider = new FakeWmiProvider("Win32_SystemDriver", new[] { DriverRow() });
        var results = await provider.ToSafeSystemDriverMetricsAsync(CancellationToken.None);

        Assert.Equal("Running", results[0].State);
    }

    [Fact]
    public async Task FullData_Maps_Started_True()
    {
        var provider = new FakeWmiProvider("Win32_SystemDriver", new[] { DriverRow() });
        var results = await provider.ToSafeSystemDriverMetricsAsync(CancellationToken.None);

        Assert.True(results[0].Started);
    }

    [Fact]
    public async Task FullData_Maps_TagId_Uint()
    {
        var provider = new FakeWmiProvider("Win32_SystemDriver", new[] { DriverRow() });
        var results = await provider.ToSafeSystemDriverMetricsAsync(CancellationToken.None);

        Assert.Equal(24u, results[0].TagId);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_SystemDriver", new[] { DriverRow() });
        var results = await provider.ToSafeSystemDriverMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_SystemDriver", new[] { DriverRow() });
        var results = await provider.ToSafeSystemDriverMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2021, 5, 5, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_SystemDriver", WmiRow.Empty());
        var results = await provider.ToSafeSystemDriverMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeSystemDriverMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleDrivers_Returns_All()
    {
        var d1 = WmiRow.Build(("Name", new WmiValue("LxssManager")));
        var d2 = WmiRow.Build(("Name", new WmiValue("vwifibus")));

        var provider = new FakeWmiProvider("Win32_SystemDriver", new[] { d1, d2 });
        var results = await provider.ToSafeSystemDriverMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("LxssManager", results[0].Name);
        Assert.Equal("vwifibus", results[1].Name);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Driver Without Path")));

        var provider = new FakeWmiProvider("Win32_SystemDriver", new[] { partial });
        var results = await provider.ToSafeSystemDriverMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Driver Without Path", results[0].Name);
        Assert.Null(results[0].PathName);
        Assert.Null(results[0].TagId);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // Started stored as a string instead of Bool — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("Started", new WmiValue("true")));

        var provider = new FakeWmiProvider("Win32_SystemDriver", new[] { badRow });
        var results = await provider.ToSafeSystemDriverMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Started);
    }
}
