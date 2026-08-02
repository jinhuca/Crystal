using Crystal.Provider.Mmi.HardwareFeatures.SerialPortSetting;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public sealed class SerialPortSettingExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> Com1Setting()
        => WmiRow.Build(
            ("Element", new WmiValue(@"Win32_SerialPort.DeviceID=""COM1""")),
            ("Setting", new WmiValue(@"Win32_SerialPortConfiguration.SettingID=""COM1"""))
        );

    [Fact]
    public async Task FullData_Maps_Element()
    {
        var provider = new FakeWmiProvider("Win32_SerialPortSetting", new[] { Com1Setting() });

        var results = await provider.ToSafeSerialPortSettingMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal(@"Win32_SerialPort.DeviceID=""COM1""", results[0].Element);
    }

    [Fact]
    public async Task FullData_Maps_Setting()
    {
        var provider = new FakeWmiProvider("Win32_SerialPortSetting", new[] { Com1Setting() });

        var results = await provider.ToSafeSerialPortSettingMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal(@"Win32_SerialPortConfiguration.SettingID=""COM1""", results[0].Setting);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_SerialPortSetting", WmiRow.Empty());

        var results = await provider.ToSafeSerialPortSettingMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());

        var results = await provider.ToSafeSerialPortSettingMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleAssociations_Returns_All()
    {
        var com1 = WmiRow.Build(
            ("Element", new WmiValue(@"Win32_SerialPort.DeviceID=""COM1""")),
            ("Setting", new WmiValue(@"Win32_SerialPortConfiguration.SettingID=""COM1""")));

        var com2 = WmiRow.Build(
            ("Element", new WmiValue(@"Win32_SerialPort.DeviceID=""COM2""")),
            ("Setting", new WmiValue(@"Win32_SerialPortConfiguration.SettingID=""COM2""")));

        var provider = new FakeWmiProvider("Win32_SerialPortSetting", new[] { com1, com2 });

        var results = await provider.ToSafeSerialPortSettingMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal(@"Win32_SerialPort.DeviceID=""COM1""", results[0].Element);
        Assert.Equal(@"Win32_SerialPort.DeviceID=""COM2""", results[1].Element);
        Assert.Equal(@"Win32_SerialPortConfiguration.SettingID=""COM1""", results[0].Setting);
        Assert.Equal(@"Win32_SerialPortConfiguration.SettingID=""COM2""", results[1].Setting);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Element", new WmiValue(@"Win32_SerialPort.DeviceID=""COM1""")));
        var provider = new FakeWmiProvider("Win32_SerialPortSetting", new[] { partial });

        var results = await provider.ToSafeSerialPortSettingMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal(@"Win32_SerialPort.DeviceID=""COM1""", results[0].Element);
        Assert.Null(results[0].Setting);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        var badRow = WmiRow.Build(("Element", new WmiValue(true)), ("Setting", new WmiValue(123)));
        var provider = new FakeWmiProvider("Win32_SerialPortSetting", new[] { badRow });

        var results = await provider.ToSafeSerialPortSettingMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Element);
        Assert.Null(results[0].Setting);
    }

    [Fact]
    public async Task Two_Identical_Records_Are_Equal()
    {
        var provider1 = new FakeWmiProvider("Win32_SerialPortSetting", new[] { Com1Setting() });
        var provider2 = new FakeWmiProvider("Win32_SerialPortSetting", new[] { Com1Setting() });

        var r1 = (await provider1.ToSafeSerialPortSettingMetricsAsync(CancellationToken.None))[0];
        var r2 = (await provider2.ToSafeSerialPortSettingMetricsAsync(CancellationToken.None))[0];

        Assert.Equal(r1, r2);
    }
}
