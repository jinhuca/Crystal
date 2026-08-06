using Crystal.Provider.Mmi.HardwareFeatures.DeviceSettings;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class DeviceSettingsExtensionsTests
{
    private const string ElementPath = "Win32_SerialPort.DeviceID=\"COM1\"";
    private const string SettingPath = "Win32_SerialPortConfiguration.SettingID=\"COM1\"";

    [Fact]
    public async Task FullData_Maps_Element_And_Setting()
    {
        var row = WmiRow.Build(
            ("Element", new WmiValue(ElementPath)),
            ("Setting", new WmiValue(SettingPath)));

        var provider = new FakeWmiProvider("Win32_DeviceSettings", new[] { row });
        var results = await provider.ToSafeDeviceSettingsMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal(ElementPath, results[0].Element);
        Assert.Equal(SettingPath, results[0].Setting);
    }

    [Fact]
    public async Task FullData_Extracts_ElementDeviceId()
    {
        var row = WmiRow.Build(("Element", new WmiValue(ElementPath)));

        var provider = new FakeWmiProvider("Win32_DeviceSettings", new[] { row });
        var results = await provider.ToSafeDeviceSettingsMetricsAsync(CancellationToken.None);

        Assert.Equal("COM1", results[0].ElementDeviceId);
    }

    [Fact]
    public async Task FullData_Extracts_SettingId()
    {
        var row = WmiRow.Build(("Setting", new WmiValue(SettingPath)));

        var provider = new FakeWmiProvider("Win32_DeviceSettings", new[] { row });
        var results = await provider.ToSafeDeviceSettingsMetricsAsync(CancellationToken.None);

        Assert.Equal("COM1", results[0].SettingId);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_DeviceSettings", WmiRow.Empty());
        var results = await provider.ToSafeDeviceSettingsMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeDeviceSettingsMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleRelationships_Returns_All()
    {
        var rel1 = WmiRow.Build(("Element", new WmiValue("Win32_SerialPort.DeviceID=\"COM1\"")));
        var rel2 = WmiRow.Build(("Element", new WmiValue("Win32_SerialPort.DeviceID=\"COM2\"")));

        var provider = new FakeWmiProvider("Win32_DeviceSettings", new[] { rel1, rel2 });
        var results = await provider.ToSafeDeviceSettingsMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("COM1", results[0].ElementDeviceId);
        Assert.Equal("COM2", results[1].ElementDeviceId);
    }

    [Fact]
    public async Task MissingReference_Leaves_Field_Null()
    {
        var partial = WmiRow.Build(("Element", new WmiValue(ElementPath)));

        var provider = new FakeWmiProvider("Win32_DeviceSettings", new[] { partial });
        var results = await provider.ToSafeDeviceSettingsMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Setting);
        Assert.Null(results[0].SettingId);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // Element stored as an int instead of String — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("Element", new WmiValue(1)));

        var provider = new FakeWmiProvider("Win32_DeviceSettings", new[] { badRow });
        var results = await provider.ToSafeDeviceSettingsMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Element);
        Assert.Null(results[0].ElementDeviceId);
    }
}
