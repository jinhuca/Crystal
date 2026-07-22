using Crystal.Mmi.HardwareFeatures.PnPEntity;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class PnPEntityExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> KeyboardRow() => WmiRow.Build(
        ("Availability", new WmiValue(3)),
        ("Caption", new WmiValue("HID Keyboard Device")),
        ("ClassGuid", new WmiValue("{4D36E96B-E325-11CE-BFC1-08002BE10318}")),
        ("CompatibleID", new WmiValue(new[] { "HID_DEVICE_SYSTEM_KEYBOARD", "HID_DEVICE_UP:0001_U:0006" })),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_PnPEntity")),
        ("Description", new WmiValue("HID Keyboard Device")),
        ("DeviceID", new WmiValue("HID\\VID_04D9&PID_1702\\7&16C6B9F6&0&0000")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("HardwareID", new WmiValue(new[] { "HID\\VID_04D9&PID_1702&REV_0100", "HID\\VID_04D9&PID_1702" })),
        ("InstallationDate", new WmiValue(new DateTime(2021, 6, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("Manufacturer", new WmiValue("(Standard keyboards)")),
        ("Name", new WmiValue("HID Keyboard Device")),
        ("PNPClass", new WmiValue("Keyboard")),
        ("PNPDeviceID", new WmiValue("HID\\VID_04D9&PID_1702\\7&16C6B9F6&0&0000")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1, 2 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("Present", new WmiValue(true)),
        ("Service", new WmiValue("kbdhid")),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01"))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { KeyboardRow() });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("HID Keyboard Device", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_Manufacturer()
    {
        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { KeyboardRow() });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.Equal("(Standard keyboards)", results[0].Manufacturer);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { KeyboardRow() });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_DeviceID()
    {
        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { KeyboardRow() });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.Equal("HID\\VID_04D9&PID_1702\\7&16C6B9F6&0&0000", results[0].DeviceID);
    }

    [Fact]
    public async Task FullData_Maps_PNPClass()
    {
        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { KeyboardRow() });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.Equal("Keyboard", results[0].PNPClass);
    }

    [Fact]
    public async Task FullData_Maps_Present_True()
    {
        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { KeyboardRow() });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.True(results[0].Present);
    }

    [Fact]
    public async Task FullData_Maps_Service()
    {
        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { KeyboardRow() });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.Equal("kbdhid", results[0].Service);
    }

    [Fact]
    public async Task FullData_Maps_ConfigManagerUserConfig_False()
    {
        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { KeyboardRow() });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.False(results[0].ConfigManagerUserConfig);
    }

    [Fact]
    public async Task FullData_Maps_ConfigManagerErrorCode_Uint()
    {
        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { KeyboardRow() });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)0, results[0].ConfigManagerErrorCode);
    }

    [Fact]
    public async Task FullData_Maps_Availability_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { KeyboardRow() });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].Availability);
    }

    [Fact]
    public async Task FullData_Maps_StatusInfo_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { KeyboardRow() });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].StatusInfo);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { KeyboardRow() });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2021, 6, 1, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task FullData_Maps_PowerManagementCapabilities_Array()
    {
        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { KeyboardRow() });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.Equal(new ushort[] { 1, 2 }, results[0].PowerManagementCapabilities);
    }

    [Fact]
    public async Task CompatibleID_Flattened_From_StringArray()
    {
        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { KeyboardRow() });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.Equal("HID_DEVICE_SYSTEM_KEYBOARD, HID_DEVICE_UP:0001_U:0006", results[0].CompatibleID);
    }

    [Fact]
    public async Task HardwareID_Flattened_From_StringArray()
    {
        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { KeyboardRow() });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.Equal("HID\\VID_04D9&PID_1702&REV_0100, HID\\VID_04D9&PID_1702", results[0].HardwareID);
    }

    [Fact]
    public async Task Multiple_Devices_Returns_All()
    {
        var device1 = WmiRow.Build(("Name", new WmiValue("Device 1")), ("PNPClass", new WmiValue("Keyboard")));
        var device2 = WmiRow.Build(("Name", new WmiValue("Device 2")), ("PNPClass", new WmiValue("Mouse")));
        var device3 = WmiRow.Build(("Name", new WmiValue("Device 3")), ("PNPClass", new WmiValue("USB")));

        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { device1, device2, device3 });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.Equal(3, results.Count);
        Assert.Equal("Device 1", results[0].Name);
        Assert.Equal("Device 2", results[1].Name);
        Assert.Equal("Device 3", results[2].Name);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_PnPEntity", WmiRow.Empty());
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task Cancelled_Token_Returns_Empty_Fallback()
    {
        // PnPEntity extension uses generic catch — swallows OperationCanceledException
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { KeyboardRow() });
        var results = await provider.ToSafePnPEntityMetricsAsync(cts.Token);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingKeys_Return_Null_For_Optional_Fields()
    {
        var row = WmiRow.Build(
            ("Name", new WmiValue("Minimal Device")),
            ("Status", new WmiValue("OK")));
        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { row });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.Equal("Minimal Device", results[0].Name);
        Assert.Equal("OK", results[0].Status);
        Assert.Null(results[0].Manufacturer);
        Assert.Null(results[0].PNPClass);
        Assert.Null(results[0].Service);
        Assert.Null(results[0].Present);
        Assert.Null(results[0].CompatibleID);
        Assert.Null(results[0].HardwareID);
    }

    [Fact]
    public async Task ClassGuid_Maps_Correctly()
    {
        var provider = new FakeWmiProvider("Win32_PnPEntity", new[] { KeyboardRow() });
        var results = await provider.ToSafePnPEntityMetricsAsync(CancellationToken.None);

        Assert.Equal("{4D36E96B-E325-11CE-BFC1-08002BE10318}", results[0].ClassGuid);
    }
}
