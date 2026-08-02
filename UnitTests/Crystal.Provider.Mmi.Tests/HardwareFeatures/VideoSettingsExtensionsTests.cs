using Crystal.Provider.Mmi.HardwareFeatures.VideoSettings;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class VideoSettingsExtensionsTests
{
    private const string ElementPath = "Win32_VideoController.DeviceID=\"VideoController1\"";
    private const string SettingPath = "CIM_VideoControllerResolution.SettingID=\"1920 x 1080 x 32 colors\"";

    [Fact]
    public async Task FullData_Maps_Element_And_Setting()
    {
        var row = WmiRow.Build(
            ("Element", new WmiValue(ElementPath)),
            ("Setting", new WmiValue(SettingPath)));

        var provider = new FakeWmiProvider("Win32_VideoSettings", new[] { row });
        var results = await provider.ToSafeVideoSettingsMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal(ElementPath, results[0].Element);
        Assert.Equal(SettingPath, results[0].Setting);
    }

    [Fact]
    public async Task FullData_Extracts_VideoControllerDeviceId()
    {
        var row = WmiRow.Build(("Element", new WmiValue(ElementPath)));

        var provider = new FakeWmiProvider("Win32_VideoSettings", new[] { row });
        var results = await provider.ToSafeVideoSettingsMetricsAsync(CancellationToken.None);

        Assert.Equal("VideoController1", results[0].VideoControllerDeviceId);
    }

    [Fact]
    public async Task FullData_Extracts_SettingId()
    {
        var row = WmiRow.Build(("Setting", new WmiValue(SettingPath)));

        var provider = new FakeWmiProvider("Win32_VideoSettings", new[] { row });
        var results = await provider.ToSafeVideoSettingsMetricsAsync(CancellationToken.None);

        Assert.Equal("1920 x 1080 x 32 colors", results[0].SettingId);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_VideoSettings", WmiRow.Empty());
        var results = await provider.ToSafeVideoSettingsMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeVideoSettingsMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleRelationships_Returns_All()
    {
        var rel1 = WmiRow.Build(("Element", new WmiValue("Win32_VideoController.DeviceID=\"VideoController1\"")));
        var rel2 = WmiRow.Build(("Element", new WmiValue("Win32_VideoController.DeviceID=\"VideoController2\"")));

        var provider = new FakeWmiProvider("Win32_VideoSettings", new[] { rel1, rel2 });
        var results = await provider.ToSafeVideoSettingsMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("VideoController1", results[0].VideoControllerDeviceId);
        Assert.Equal("VideoController2", results[1].VideoControllerDeviceId);
    }

    [Fact]
    public async Task MissingReference_Leaves_Field_Null()
    {
        var partial = WmiRow.Build(("Element", new WmiValue(ElementPath)));

        var provider = new FakeWmiProvider("Win32_VideoSettings", new[] { partial });
        var results = await provider.ToSafeVideoSettingsMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Setting);
        Assert.Null(results[0].SettingId);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // Setting stored as a bool instead of String — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("Setting", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_VideoSettings", new[] { badRow });
        var results = await provider.ToSafeVideoSettingsMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Setting);
        Assert.Null(results[0].SettingId);
    }
}
