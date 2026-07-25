using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.SoftwareFeatures.Desktop;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.SoftwareFeatures;

public class DesktopExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> DesktopRow() => WmiRow.Build(
        ("BorderWidth", new WmiValue(1)),
        ("Caption", new WmiValue("")),
        ("CoolSwitch", new WmiValue(true)),
        ("CursorBlinkRate", new WmiValue(530)),
        ("Description", new WmiValue("")),
        ("DragFullWindows", new WmiValue(true)),
        ("GridGranularity", new WmiValue(0)),
        ("IconSpacing", new WmiValue(75)),
        ("IconTitleFaceName", new WmiValue("Segoe UI")),
        ("IconTitleSize", new WmiValue(9)),
        ("IconTitleWrap", new WmiValue(true)),
        ("Name", new WmiValue("SOMEDOMAIN\\johndoe")),
        ("Pattern", new WmiValue("(None)")),
        ("ScreenSaverActive", new WmiValue(false)),
        ("ScreenSaverExecutable", new WmiValue("")),
        ("ScreenSaverSecure", new WmiValue(true)),
        ("ScreenSaverTimeout", new WmiValue(600)),
        ("SettingID", new WmiValue("SOMEDOMAIN\\johndoe")),
        ("Wallpaper", new WmiValue("C:\\Windows\\Web\\Wallpaper\\img0.jpg")),
        ("WallpaperStretched", new WmiValue(false)),
        ("WallpaperTiled", new WmiValue(false))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_Desktop", new[] { DesktopRow() });
        var results = await provider.ToSafeDesktopMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("SOMEDOMAIN\\johndoe", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_Wallpaper()
    {
        var provider = new FakeWmiProvider("Win32_Desktop", new[] { DesktopRow() });
        var results = await provider.ToSafeDesktopMetricsAsync(CancellationToken.None);

        Assert.Equal("C:\\Windows\\Web\\Wallpaper\\img0.jpg", results[0].Wallpaper);
    }

    [Fact]
    public async Task FullData_Maps_ScreenSaverTimeout_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Desktop", new[] { DesktopRow() });
        var results = await provider.ToSafeDesktopMetricsAsync(CancellationToken.None);

        Assert.Equal(600u, results[0].ScreenSaverTimeout);
    }

    [Fact]
    public async Task FullData_Maps_ScreenSaverActive_False()
    {
        var provider = new FakeWmiProvider("Win32_Desktop", new[] { DesktopRow() });
        var results = await provider.ToSafeDesktopMetricsAsync(CancellationToken.None);

        Assert.False(results[0].ScreenSaverActive);
    }

    [Fact]
    public async Task FullData_Maps_SettingID()
    {
        var provider = new FakeWmiProvider("Win32_Desktop", new[] { DesktopRow() });
        var results = await provider.ToSafeDesktopMetricsAsync(CancellationToken.None);

        Assert.Equal("SOMEDOMAIN\\johndoe", results[0].SettingID);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_Desktop", WmiRow.Empty());
        var results = await provider.ToSafeDesktopMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeDesktopMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleDesktops_Returns_All()
    {
        var d1 = WmiRow.Build(("Name", new WmiValue("SOMEDOMAIN\\johndoe")));
        var d2 = WmiRow.Build(("Name", new WmiValue("SOMEDOMAIN\\janedoe")));

        var provider = new FakeWmiProvider("Win32_Desktop", new[] { d1, d2 });
        var results = await provider.ToSafeDesktopMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("SOMEDOMAIN\\johndoe", results[0].Name);
        Assert.Equal("SOMEDOMAIN\\janedoe", results[1].Name);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Desktop Without Wallpaper")));

        var provider = new FakeWmiProvider("Win32_Desktop", new[] { partial });
        var results = await provider.ToSafeDesktopMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Desktop Without Wallpaper", results[0].Name);
        Assert.Null(results[0].Wallpaper);
        Assert.Null(results[0].ScreenSaverTimeout);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // ScreenSaverTimeout stored as a string instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("ScreenSaverTimeout", new WmiValue("600")));

        var provider = new FakeWmiProvider("Win32_Desktop", new[] { badRow });
        var results = await provider.ToSafeDesktopMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].ScreenSaverTimeout);
    }
}
