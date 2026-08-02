using Crystal.Provider.Mmi.HardwareFeatures.Keyboard;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class KeyboardExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> KeyboardRow() => WmiRow.Build(
        ("Availability", new WmiValue(3)),
        ("Caption", new WmiValue("Enhanced (101- or 102-key)")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_Keyboard")),
        ("Description", new WmiValue("PC/AT Enhanced PS/2 Keyboard (101/102-Key)")),
        ("DeviceID", new WmiValue("HID\\VID_046D&PID_C31C\\6&1234")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("InstallDate", new WmiValue(new DateTime(2022, 4, 12, 0, 0, 0, DateTimeKind.Utc))),
        ("IsLocked", new WmiValue(false)),
        ("LastErrorCode", new WmiValue(0)),
        ("Layout", new WmiValue("00000409")),
        ("Name", new WmiValue("Enhanced (101- or 102-key)")),
        ("NumberOfFunctionKeys", new WmiValue(12)),
        ("Password", new WmiValue(2)),
        ("PNPDeviceID", new WmiValue("HID\\VID_046D&PID_C31C\\6&1234")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01"))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_Keyboard", new[] { KeyboardRow() });
        var results = await provider.ToSafeKeyboardMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Enhanced (101- or 102-key)", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_DeviceID()
    {
        var provider = new FakeWmiProvider("Win32_Keyboard", new[] { KeyboardRow() });
        var results = await provider.ToSafeKeyboardMetricsAsync(CancellationToken.None);

        Assert.Equal("HID\\VID_046D&PID_C31C\\6&1234", results[0].DeviceID);
    }

    [Fact]
    public async Task FullData_Maps_NumberOfFunctionKeys_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_Keyboard", new[] { KeyboardRow() });
        var results = await provider.ToSafeKeyboardMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)12, results[0].NumberOfFunctionKeys);
    }

    [Fact]
    public async Task FullData_Maps_IsLocked_False()
    {
        var provider = new FakeWmiProvider("Win32_Keyboard", new[] { KeyboardRow() });
        var results = await provider.ToSafeKeyboardMetricsAsync(CancellationToken.None);

        Assert.False(results[0].IsLocked);
    }

    [Fact]
    public async Task FullData_Maps_Layout()
    {
        var provider = new FakeWmiProvider("Win32_Keyboard", new[] { KeyboardRow() });
        var results = await provider.ToSafeKeyboardMetricsAsync(CancellationToken.None);

        Assert.Equal("00000409", results[0].Layout);
    }

    [Fact]
    public async Task FullData_Maps_Password_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_Keyboard", new[] { KeyboardRow() });
        var results = await provider.ToSafeKeyboardMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)2, results[0].Password);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_Keyboard", new[] { KeyboardRow() });
        var results = await provider.ToSafeKeyboardMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2022, 4, 12, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_Keyboard", new[] { KeyboardRow() });
        var results = await provider.ToSafeKeyboardMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_Keyboard", WmiRow.Empty());
        var results = await provider.ToSafeKeyboardMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeKeyboardMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleKeyboards_Returns_All()
    {
        var kb1 = WmiRow.Build(("DeviceID", new WmiValue("KB1")), ("Name", new WmiValue("Keyboard 1")));
        var kb2 = WmiRow.Build(("DeviceID", new WmiValue("KB2")), ("Name", new WmiValue("Keyboard 2")));

        var provider = new FakeWmiProvider("Win32_Keyboard", new[] { kb1, kb2 });
        var results = await provider.ToSafeKeyboardMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Keyboard 1", results[0].Name);
        Assert.Equal("Keyboard 2", results[1].Name);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Keyboard Without Layout")));

        var provider = new FakeWmiProvider("Win32_Keyboard", new[] { partial });
        var results = await provider.ToSafeKeyboardMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Keyboard Without Layout", results[0].Name);
        Assert.Null(results[0].Layout);
        Assert.Null(results[0].NumberOfFunctionKeys);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // NumberOfFunctionKeys stored as a string instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("NumberOfFunctionKeys", new WmiValue("12")));

        var provider = new FakeWmiProvider("Win32_Keyboard", new[] { badRow });
        var results = await provider.ToSafeKeyboardMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].NumberOfFunctionKeys);
    }
}
