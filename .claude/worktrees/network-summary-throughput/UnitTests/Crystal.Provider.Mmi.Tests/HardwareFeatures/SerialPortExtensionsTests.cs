using Crystal.Provider.Mmi.HardwareFeatures.SerialPort;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class SerialPortExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> Com1Row() => WmiRow.Build(
        ("Availability", new WmiValue(3)),
        ("Binary", new WmiValue(true)),
        ("Capabilities", new WmiValue(new ushort[] { 5 })),
        ("CapabilityDescriptions", new WmiValue(new[] { "16550A Compatible" })),
        ("Caption", new WmiValue("Communications Port (COM1)")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_SerialPort")),
        ("Description", new WmiValue("Communications Port")),
        ("DeviceID", new WmiValue("COM1")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("InstallDate", new WmiValue(new DateTime(2023, 1, 15, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("MaxBaudRate", new WmiValue(115200)),
        ("MaximumInputBufferSize", new WmiValue(4096)),
        ("MaximumOutputBufferSize", new WmiValue(2048)),
        ("MaxNumberControlled", new WmiValue(0)),
        ("Name", new WmiValue("Communications Port (COM1)")),
        ("OSAutoDiscovered", new WmiValue(true)),
        ("PNPDeviceID", new WmiValue("ACPI\\PNP0501\\1")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("ProtocolSupported", new WmiValue(27)),
        ("ProviderType", new WmiValue("RS232 Serial Port")),
        ("SettableBaudRate", new WmiValue(true)),
        ("SettableDataBits", new WmiValue(true)),
        ("SettableFlowControl", new WmiValue(true)),
        ("SettableParity", new WmiValue(true)),
        ("SettableParityCheck", new WmiValue(true)),
        ("SettableRLSD", new WmiValue(false)),
        ("SettableStopBits", new WmiValue(true)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("Supports16BitMode", new WmiValue(true)),
        ("SupportsDTRDSR", new WmiValue(true)),
        ("SupportsElapsedTimeouts", new WmiValue(true)),
        ("SupportsIntTimeouts", new WmiValue(true)),
        ("SupportsParityCheck", new WmiValue(true)),
        ("SupportsRLSD", new WmiValue(false)),
        ("SupportsRTSCTS", new WmiValue(true)),
        ("SupportsSpecialCharacters", new WmiValue(true)),
        ("SupportsXOnXOff", new WmiValue(true)),
        ("SupportsXOnXOffSet", new WmiValue(true)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("TimeOfLastReset", new WmiValue(new DateTime(2024, 6, 1, 8, 30, 0, DateTimeKind.Utc)))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_SerialPort", new[] { Com1Row() });
        var results = await provider.ToSafeSerialPortMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Communications Port (COM1)", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_DeviceID()
    {
        var provider = new FakeWmiProvider("Win32_SerialPort", new[] { Com1Row() });
        var results = await provider.ToSafeSerialPortMetricsAsync(CancellationToken.None);

        Assert.Equal("COM1", results[0].DeviceID);
    }

    [Fact]
    public async Task FullData_Maps_Availability_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_SerialPort", new[] { Com1Row() });
        var results = await provider.ToSafeSerialPortMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].Availability);
    }

    [Fact]
    public async Task FullData_Maps_MaxBaudRate_Uint()
    {
        var provider = new FakeWmiProvider("Win32_SerialPort", new[] { Com1Row() });
        var results = await provider.ToSafeSerialPortMetricsAsync(CancellationToken.None);

        Assert.Equal(115200u, results[0].MaxBaudRate);
    }

    [Fact]
    public async Task FullData_Maps_Binary_True()
    {
        var provider = new FakeWmiProvider("Win32_SerialPort", new[] { Com1Row() });
        var results = await provider.ToSafeSerialPortMetricsAsync(CancellationToken.None);

        Assert.True(results[0].Binary);
    }

    [Fact]
    public async Task FullData_Maps_Capabilities_Array()
    {
        var provider = new FakeWmiProvider("Win32_SerialPort", new[] { Com1Row() });
        var results = await provider.ToSafeSerialPortMetricsAsync(CancellationToken.None);

        Assert.Equal(new ushort[] { 5 }, results[0].Capabilities);
    }

    [Fact]
    public async Task FullData_Maps_CapabilityDescriptions_Flattened()
    {
        var provider = new FakeWmiProvider("Win32_SerialPort", new[] { Com1Row() });
        var results = await provider.ToSafeSerialPortMetricsAsync(CancellationToken.None);

        Assert.Equal("16550A Compatible", results[0].CapabilityDescriptions);
    }

    [Fact]
    public async Task FullData_Maps_ProtocolSupported_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_SerialPort", new[] { Com1Row() });
        var results = await provider.ToSafeSerialPortMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)27, results[0].ProtocolSupported);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_SerialPort", new[] { Com1Row() });
        var results = await provider.ToSafeSerialPortMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_SerialPort", new[] { Com1Row() });
        var results = await provider.ToSafeSerialPortMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2023, 1, 15, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task FullData_Maps_TimeOfLastReset()
    {
        var provider = new FakeWmiProvider("Win32_SerialPort", new[] { Com1Row() });
        var results = await provider.ToSafeSerialPortMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2024, 6, 1, 8, 30, 0, DateTimeKind.Utc), results[0].TimeOfLastReset);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_SerialPort", WmiRow.Empty());
        var results = await provider.ToSafeSerialPortMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeSerialPortMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleSerialPorts_Returns_All()
    {
        var com1 = WmiRow.Build(("DeviceID", new WmiValue("COM1")), ("Name", new WmiValue("Communications Port (COM1)")));
        var com2 = WmiRow.Build(("DeviceID", new WmiValue("COM2")), ("Name", new WmiValue("Communications Port (COM2)")));

        var provider = new FakeWmiProvider("Win32_SerialPort", new[] { com1, com2 });
        var results = await provider.ToSafeSerialPortMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Communications Port (COM1)", results[0].Name);
        Assert.Equal("Communications Port (COM2)", results[1].Name);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Port Without Baud Rate")));

        var provider = new FakeWmiProvider("Win32_SerialPort", new[] { partial });
        var results = await provider.ToSafeSerialPortMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Port Without Baud Rate", results[0].Name);
        Assert.Null(results[0].MaxBaudRate);
        Assert.Null(results[0].Capabilities);
        Assert.Null(results[0].CapabilityDescriptions);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // MaxBaudRate stored as a string instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("MaxBaudRate", new WmiValue("not-a-number")));

        var provider = new FakeWmiProvider("Win32_SerialPort", new[] { badRow });
        var results = await provider.ToSafeSerialPortMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].MaxBaudRate);
    }
}
