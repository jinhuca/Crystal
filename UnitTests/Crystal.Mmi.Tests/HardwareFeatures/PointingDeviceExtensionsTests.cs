using Crystal.Mmi.HardwareFeatures.PointingDevice;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class PointingDeviceExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> MouseRow() => WmiRow.Build(
        ("Availability", new WmiValue(3)),
        ("Caption", new WmiValue("Microsoft PS/2 Mouse")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_PointingDevice")),
        ("Description", new WmiValue("Microsoft PS/2 Mouse")),
        ("DeviceID", new WmiValue("ROOT\\*PNP0F03\\1_0_21_0_31_0")),
        ("DeviceInterface", new WmiValue(4)),
        ("DoubleSpeedThreshold", new WmiValue(6)),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("Handedness", new WmiValue(0)),
        ("HardwareType", new WmiValue("MICROSOFT PS2 MOUSE")),
        ("InfFileName", new WmiValue("mouclass.inf")),
        ("InfSection", new WmiValue("PS2_Mouse")),
        ("InstallDate", new WmiValue(new DateTime(2022, 7, 4, 0, 0, 0, DateTimeKind.Utc))),
        ("IsLocked", new WmiValue(false)),
        ("LastErrorCode", new WmiValue(0)),
        ("Manufacturer", new WmiValue("Microsoft")),
        ("Name", new WmiValue("Microsoft PS/2 Mouse")),
        ("NumberOfButtons", new WmiValue(2)),
        ("PNPDeviceID", new WmiValue("ROOT\\*PNP0F03\\1_0_21_0_31_0")),
        ("PointingType", new WmiValue(3)),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("QuadSpeedThreshold", new WmiValue(10)),
        ("Resolution", new WmiValue(0)),
        ("SampleRate", new WmiValue(40)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("Synch", new WmiValue(100)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01"))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_PointingDevice", new[] { MouseRow() });
        var results = await provider.ToSafePointingDeviceMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Microsoft PS/2 Mouse", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_NumberOfButtons_Byte()
    {
        var provider = new FakeWmiProvider("Win32_PointingDevice", new[] { MouseRow() });
        var results = await provider.ToSafePointingDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal((byte)2, results[0].NumberOfButtons);
    }

    [Fact]
    public async Task FullData_Maps_PointingType_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_PointingDevice", new[] { MouseRow() });
        var results = await provider.ToSafePointingDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].PointingType);
    }

    [Fact]
    public async Task FullData_Maps_Manufacturer()
    {
        var provider = new FakeWmiProvider("Win32_PointingDevice", new[] { MouseRow() });
        var results = await provider.ToSafePointingDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("Microsoft", results[0].Manufacturer);
    }

    [Fact]
    public async Task FullData_Maps_HardwareType()
    {
        var provider = new FakeWmiProvider("Win32_PointingDevice", new[] { MouseRow() });
        var results = await provider.ToSafePointingDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("MICROSOFT PS2 MOUSE", results[0].HardwareType);
    }

    [Fact]
    public async Task FullData_Maps_IsLocked_False()
    {
        var provider = new FakeWmiProvider("Win32_PointingDevice", new[] { MouseRow() });
        var results = await provider.ToSafePointingDeviceMetricsAsync(CancellationToken.None);

        Assert.False(results[0].IsLocked);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_PointingDevice", new[] { MouseRow() });
        var results = await provider.ToSafePointingDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_PointingDevice", new[] { MouseRow() });
        var results = await provider.ToSafePointingDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2022, 7, 4, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_PointingDevice", WmiRow.Empty());
        var results = await provider.ToSafePointingDeviceMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafePointingDeviceMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleDevices_Returns_All()
    {
        var dev1 = WmiRow.Build(("DeviceID", new WmiValue("MOUSE1")), ("Name", new WmiValue("Mouse 1")));
        var dev2 = WmiRow.Build(("DeviceID", new WmiValue("MOUSE2")), ("Name", new WmiValue("Mouse 2")));

        var provider = new FakeWmiProvider("Win32_PointingDevice", new[] { dev1, dev2 });
        var results = await provider.ToSafePointingDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Mouse 1", results[0].Name);
        Assert.Equal("Mouse 2", results[1].Name);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Device Without Buttons")));

        var provider = new FakeWmiProvider("Win32_PointingDevice", new[] { partial });
        var results = await provider.ToSafePointingDeviceMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Device Without Buttons", results[0].Name);
        Assert.Null(results[0].NumberOfButtons);
        Assert.Null(results[0].PointingType);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // NumberOfButtons stored as a string instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("NumberOfButtons", new WmiValue("2")));

        var provider = new FakeWmiProvider("Win32_PointingDevice", new[] { badRow });
        var results = await provider.ToSafePointingDeviceMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].NumberOfButtons);
    }
}
