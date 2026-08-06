using Crystal.Provider.Mmi.HardwareFeatures.DisplayControllerConfiguration;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class DisplayControllerConfigurationExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> ConfigRow() => WmiRow.Build(
        ("BitsPerPixel", new WmiValue(32)),
        ("Caption", new WmiValue("NVIDIA GeForce RTX 3080")),
        ("ColorPlanes", new WmiValue(1)),
        ("Description", new WmiValue("NVIDIA GeForce RTX 3080")),
        ("DeviceEntriesInAColorTable", new WmiValue(-1)),
        ("DeviceSpecificPens", new WmiValue(3)),
        ("HorizontalResolution", new WmiValue(1920)),
        ("Name", new WmiValue("NVIDIA GeForce RTX 3080")),
        ("RefreshRate", new WmiValue(-1)),
        ("ReservedSystemPaletteEntries", new WmiValue(0)),
        ("SettingID", new WmiValue("NVIDIA GeForce RTX 3080")),
        ("SystemPaletteEntries", new WmiValue(0)),
        ("VerticalResolution", new WmiValue(1080)),
        ("VideoMode", new WmiValue("1920 x 1080 with 4294967296 colors"))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_DisplayControllerConfiguration", new[] { ConfigRow() });
        var results = await provider.ToSafeDisplayControllerConfigurationMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("NVIDIA GeForce RTX 3080", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_HorizontalResolution_Uint()
    {
        var provider = new FakeWmiProvider("Win32_DisplayControllerConfiguration", new[] { ConfigRow() });
        var results = await provider.ToSafeDisplayControllerConfigurationMetricsAsync(CancellationToken.None);

        Assert.Equal(1920u, results[0].HorizontalResolution);
    }

    [Fact]
    public async Task FullData_Maps_VerticalResolution_Uint()
    {
        var provider = new FakeWmiProvider("Win32_DisplayControllerConfiguration", new[] { ConfigRow() });
        var results = await provider.ToSafeDisplayControllerConfigurationMetricsAsync(CancellationToken.None);

        Assert.Equal(1080u, results[0].VerticalResolution);
    }

    [Fact]
    public async Task FullData_Maps_RefreshRate_NegativeInt()
    {
        var provider = new FakeWmiProvider("Win32_DisplayControllerConfiguration", new[] { ConfigRow() });
        var results = await provider.ToSafeDisplayControllerConfigurationMetricsAsync(CancellationToken.None);

        Assert.Equal(-1, results[0].RefreshRate);
    }

    [Fact]
    public async Task FullData_Maps_BitsPerPixel_Uint()
    {
        var provider = new FakeWmiProvider("Win32_DisplayControllerConfiguration", new[] { ConfigRow() });
        var results = await provider.ToSafeDisplayControllerConfigurationMetricsAsync(CancellationToken.None);

        Assert.Equal(32u, results[0].BitsPerPixel);
    }

    [Fact]
    public async Task FullData_Maps_VideoMode()
    {
        var provider = new FakeWmiProvider("Win32_DisplayControllerConfiguration", new[] { ConfigRow() });
        var results = await provider.ToSafeDisplayControllerConfigurationMetricsAsync(CancellationToken.None);

        Assert.Equal("1920 x 1080 with 4294967296 colors", results[0].VideoMode);
    }

    [Fact]
    public async Task FullData_Maps_SettingID()
    {
        var provider = new FakeWmiProvider("Win32_DisplayControllerConfiguration", new[] { ConfigRow() });
        var results = await provider.ToSafeDisplayControllerConfigurationMetricsAsync(CancellationToken.None);

        Assert.Equal("NVIDIA GeForce RTX 3080", results[0].SettingID);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_DisplayControllerConfiguration", WmiRow.Empty());
        var results = await provider.ToSafeDisplayControllerConfigurationMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeDisplayControllerConfigurationMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleConfigurations_Returns_All()
    {
        var cfg1 = WmiRow.Build(("Name", new WmiValue("Adapter 1")));
        var cfg2 = WmiRow.Build(("Name", new WmiValue("Adapter 2")));

        var provider = new FakeWmiProvider("Win32_DisplayControllerConfiguration", new[] { cfg1, cfg2 });
        var results = await provider.ToSafeDisplayControllerConfigurationMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Adapter 1", results[0].Name);
        Assert.Equal("Adapter 2", results[1].Name);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Adapter Without Resolution")));

        var provider = new FakeWmiProvider("Win32_DisplayControllerConfiguration", new[] { partial });
        var results = await provider.ToSafeDisplayControllerConfigurationMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Adapter Without Resolution", results[0].Name);
        Assert.Null(results[0].HorizontalResolution);
        Assert.Null(results[0].VerticalResolution);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // HorizontalResolution stored as a bool instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("HorizontalResolution", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_DisplayControllerConfiguration", new[] { badRow });
        var results = await provider.ToSafeDisplayControllerConfigurationMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].HorizontalResolution);
    }
}
