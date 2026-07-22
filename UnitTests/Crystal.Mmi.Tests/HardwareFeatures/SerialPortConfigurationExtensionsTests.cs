using Crystal.Mmi.HardwareFeatures.SerialPortConfiguration;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public sealed class SerialPortConfigurationExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> Com1Config()
        => WmiRow.Build(
            ("Caption", new WmiValue("COM1 Configuration")),
            ("Description", new WmiValue("Serial Port Configuration")),
            ("Name", new WmiValue("COM1")),
            ("SettingID", new WmiValue("COM1")),
            ("AbortReadWriteOnError", new WmiValue(false)),
            ("BaudRate", new WmiValue(115200)),
            ("Binary", new WmiValue(true)),
            ("BitsPerByte", new WmiValue(8)),
            ("ContinueXMitOnXOff", new WmiValue(false)),
            ("CTSOutflowControl", new WmiValue(true)),
            ("DiscardNULL", new WmiValue(false)),
            ("DSROutflowControl", new WmiValue(false)),
            ("DSRSensitivity", new WmiValue(false)),
            ("DTRFlowControlType", new WmiValue(1)),
            ("EOFCharacter", new WmiValue(26)),
            ("ErrorReplaceCharacter", new WmiValue(63)),
            ("InFlowControlType", new WmiValue(1)),
            ("OutFlowControlType", new WmiValue(1)),
            ("Parity", new WmiValue(0)),
            ("ParityCheck", new WmiValue(false)),
            ("RTSFlowControlType", new WmiValue(1)),
            ("StopBits", new WmiValue(1)),
            ("XOffCharacter", new WmiValue(19)),
            ("XOffXMitThreshold", new WmiValue(512)),
            ("XOnCharacter", new WmiValue(17)),
            ("XOnXMitThreshold", new WmiValue(2048)),
            ("XOnXOffInFlowControl", new WmiValue(false)),
            ("XOnXOffOutFlowControl", new WmiValue(false))
        );

    [Fact]
    public async Task FullData_Maps_Core_Identity_Fields()
    {
        var provider = new FakeWmiProvider("Win32_SerialPortConfiguration", new[] { Com1Config() });

        var results = await provider.ToSafeSerialPortConfigurationMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("COM1 Configuration", results[0].Caption);
        Assert.Equal("Serial Port Configuration", results[0].Description);
        Assert.Equal("COM1", results[0].Name);
        Assert.Equal("COM1", results[0].SettingID);
    }

    [Fact]
    public async Task FullData_Maps_BaudRate_And_Format_Fields()
    {
        var provider = new FakeWmiProvider("Win32_SerialPortConfiguration", new[] { Com1Config() });

        var results = await provider.ToSafeSerialPortConfigurationMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)115200, results[0].BaudRate);
        Assert.True(results[0].Binary);
        Assert.Equal((uint)8, results[0].BitsPerByte);
        Assert.Equal((uint)0, results[0].Parity);
        Assert.False(results[0].ParityCheck);
        Assert.Equal((uint)1, results[0].StopBits);
    }

    [Fact]
    public async Task FullData_Maps_Flow_Control_Fields()
    {
        var provider = new FakeWmiProvider("Win32_SerialPortConfiguration", new[] { Com1Config() });

        var results = await provider.ToSafeSerialPortConfigurationMetricsAsync(CancellationToken.None);

        Assert.False(results[0].ContinueXMitOnXOff);
        Assert.True(results[0].CTSOutflowControl);
        Assert.False(results[0].DSROutflowControl);
        Assert.False(results[0].DSRSensitivity);
        Assert.Equal((uint)1, results[0].DTRFlowControlType);
        Assert.Equal((uint)1, results[0].InFlowControlType);
        Assert.Equal((uint)1, results[0].OutFlowControlType);
        Assert.Equal((uint)1, results[0].RTSFlowControlType);
        Assert.False(results[0].XOnXOffInFlowControl);
        Assert.False(results[0].XOnXOffOutFlowControl);
    }

    [Fact]
    public async Task FullData_Maps_Control_Character_Fields()
    {
        var provider = new FakeWmiProvider("Win32_SerialPortConfiguration", new[] { Com1Config() });

        var results = await provider.ToSafeSerialPortConfigurationMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)26, results[0].EOFCharacter);
        Assert.Equal((uint)63, results[0].ErrorReplaceCharacter);
        Assert.Equal((uint)19, results[0].XOffCharacter);
        Assert.Equal((uint)512, results[0].XOffXMitThreshold);
        Assert.Equal((uint)17, results[0].XOnCharacter);
        Assert.Equal((uint)2048, results[0].XOnXMitThreshold);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_SerialPortConfiguration", WmiRow.Empty());

        var results = await provider.ToSafeSerialPortConfigurationMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());

        var results = await provider.ToSafeSerialPortConfigurationMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleConfigurations_Returns_All()
    {
        var com1 = WmiRow.Build(("Name", new WmiValue("COM1")), ("BaudRate", new WmiValue(115200)));
        var com2 = WmiRow.Build(("Name", new WmiValue("COM2")), ("BaudRate", new WmiValue(9600)));
        var provider = new FakeWmiProvider("Win32_SerialPortConfiguration", new[] { com1, com2 });

        var results = await provider.ToSafeSerialPortConfigurationMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("COM1", results[0].Name);
        Assert.Equal("COM2", results[1].Name);
        Assert.Equal((uint)115200, results[0].BaudRate);
        Assert.Equal((uint)9600, results[1].BaudRate);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("COM1")));
        var provider = new FakeWmiProvider("Win32_SerialPortConfiguration", new[] { partial });

        var results = await provider.ToSafeSerialPortConfigurationMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("COM1", results[0].Name);
        Assert.Null(results[0].BaudRate);
        Assert.Null(results[0].Parity);
        Assert.Null(results[0].StopBits);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        var badRow = WmiRow.Build(
            ("BaudRate", new WmiValue("115200")),
            ("BitsPerByte", new WmiValue("8")),
            ("Binary", new WmiValue("true")),
            ("Name", new WmiValue(true)));
        var provider = new FakeWmiProvider("Win32_SerialPortConfiguration", new[] { badRow });

        var results = await provider.ToSafeSerialPortConfigurationMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].BaudRate);
        Assert.Null(results[0].BitsPerByte);
        Assert.Null(results[0].Binary);
        Assert.Null(results[0].Name);
    }

    [Fact]
    public async Task Two_Identical_Records_Are_Equal()
    {
        var provider1 = new FakeWmiProvider("Win32_SerialPortConfiguration", new[] { Com1Config() });
        var provider2 = new FakeWmiProvider("Win32_SerialPortConfiguration", new[] { Com1Config() });

        var r1 = (await provider1.ToSafeSerialPortConfigurationMetricsAsync(CancellationToken.None))[0];
        var r2 = (await provider2.ToSafeSerialPortConfigurationMetricsAsync(CancellationToken.None))[0];

        Assert.Equal(r1, r2);
    }
}
