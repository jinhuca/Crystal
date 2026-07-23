using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.SoftwareFeatures.StartupCommand;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.SoftwareFeatures;

public class StartupCommandExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> StartupRow() => WmiRow.Build(
        ("Caption", new WmiValue("OneDrive")),
        ("Command", new WmiValue("\"C:\\Program Files\\Microsoft OneDrive\\OneDrive.exe\" /background")),
        ("Description", new WmiValue("OneDrive")),
        ("Location", new WmiValue("HKU\\S-1-5-21-...\\...\\Run")),
        ("Name", new WmiValue("OneDrive")),
        ("SettingID", new WmiValue("SOMEDOMAIN\\johndoe:OneDrive")),
        ("User", new WmiValue("SOMEDOMAIN\\johndoe")),
        ("UserSID", new WmiValue("S-1-5-21-1579938362-1064596589-3161144252-1006"))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_StartupCommand", new[] { StartupRow() });
        var results = await provider.ToSafeStartupCommandMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("OneDrive", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_Command()
    {
        var provider = new FakeWmiProvider("Win32_StartupCommand", new[] { StartupRow() });
        var results = await provider.ToSafeStartupCommandMetricsAsync(CancellationToken.None);

        Assert.Equal("\"C:\\Program Files\\Microsoft OneDrive\\OneDrive.exe\" /background", results[0].Command);
    }

    [Fact]
    public async Task FullData_Maps_Location()
    {
        var provider = new FakeWmiProvider("Win32_StartupCommand", new[] { StartupRow() });
        var results = await provider.ToSafeStartupCommandMetricsAsync(CancellationToken.None);

        Assert.Equal("HKU\\S-1-5-21-...\\...\\Run", results[0].Location);
    }

    [Fact]
    public async Task FullData_Maps_User()
    {
        var provider = new FakeWmiProvider("Win32_StartupCommand", new[] { StartupRow() });
        var results = await provider.ToSafeStartupCommandMetricsAsync(CancellationToken.None);

        Assert.Equal("SOMEDOMAIN\\johndoe", results[0].User);
    }

    [Fact]
    public async Task FullData_Maps_UserSID()
    {
        var provider = new FakeWmiProvider("Win32_StartupCommand", new[] { StartupRow() });
        var results = await provider.ToSafeStartupCommandMetricsAsync(CancellationToken.None);

        Assert.Equal("S-1-5-21-1579938362-1064596589-3161144252-1006", results[0].UserSID);
    }

    [Fact]
    public async Task FullData_Maps_SettingID()
    {
        var provider = new FakeWmiProvider("Win32_StartupCommand", new[] { StartupRow() });
        var results = await provider.ToSafeStartupCommandMetricsAsync(CancellationToken.None);

        Assert.Equal("SOMEDOMAIN\\johndoe:OneDrive", results[0].SettingID);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_StartupCommand", WmiRow.Empty());
        var results = await provider.ToSafeStartupCommandMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeStartupCommandMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleStartupCommands_Returns_All()
    {
        var cmd1 = WmiRow.Build(("Name", new WmiValue("OneDrive")));
        var cmd2 = WmiRow.Build(("Name", new WmiValue("SecurityHealth")));

        var provider = new FakeWmiProvider("Win32_StartupCommand", new[] { cmd1, cmd2 });
        var results = await provider.ToSafeStartupCommandMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("OneDrive", results[0].Name);
        Assert.Equal("SecurityHealth", results[1].Name);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Command Without User Info")));

        var provider = new FakeWmiProvider("Win32_StartupCommand", new[] { partial });
        var results = await provider.ToSafeStartupCommandMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Command Without User Info", results[0].Name);
        Assert.Null(results[0].User);
        Assert.Null(results[0].UserSID);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // Command stored as a bool instead of String — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("Command", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_StartupCommand", new[] { badRow });
        var results = await provider.ToSafeStartupCommandMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Command);
    }
}
