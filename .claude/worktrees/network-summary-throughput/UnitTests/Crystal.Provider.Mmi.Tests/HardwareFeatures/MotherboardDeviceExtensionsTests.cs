using Crystal.Provider.Mmi.HardwareFeatures.MotherboardDevice;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class MotherboardDeviceExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> BoardRow() => WmiRow.Build(
        ("Availability", new WmiValue(3)),
        ("Caption", new WmiValue("Motherboard")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_MotherboardDevice")),
        ("Description", new WmiValue("Motherboard")),
        ("DeviceID", new WmiValue("MotherboardDevice0")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("InstallDate", new WmiValue(new DateTime(2021, 11, 20, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("Name", new WmiValue("Motherboard")),
        ("PNPDeviceID", new WmiValue("ACPI\\PNP0C02\\1")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("PrimaryBusType", new WmiValue("PCI")),
        ("RevisionNumber", new WmiValue("00")),
        ("SecondaryBusType", new WmiValue("ISA")),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01"))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_MotherboardDevice", new[] { BoardRow() });
        var results = await provider.ToSafeMotherboardDeviceMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Motherboard", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_DeviceID()
    {
        var provider = new FakeWmiProvider("Win32_MotherboardDevice", new[] { BoardRow() });
        var results = await provider.ToSafeMotherboardDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("MotherboardDevice0", results[0].DeviceID);
    }

    [Fact]
    public async Task FullData_Maps_PrimaryBusType()
    {
        var provider = new FakeWmiProvider("Win32_MotherboardDevice", new[] { BoardRow() });
        var results = await provider.ToSafeMotherboardDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("PCI", results[0].PrimaryBusType);
    }

    [Fact]
    public async Task FullData_Maps_SecondaryBusType()
    {
        var provider = new FakeWmiProvider("Win32_MotherboardDevice", new[] { BoardRow() });
        var results = await provider.ToSafeMotherboardDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("ISA", results[0].SecondaryBusType);
    }

    [Fact]
    public async Task FullData_Maps_RevisionNumber()
    {
        var provider = new FakeWmiProvider("Win32_MotherboardDevice", new[] { BoardRow() });
        var results = await provider.ToSafeMotherboardDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("00", results[0].RevisionNumber);
    }

    [Fact]
    public async Task FullData_Maps_Availability_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_MotherboardDevice", new[] { BoardRow() });
        var results = await provider.ToSafeMotherboardDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].Availability);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_MotherboardDevice", new[] { BoardRow() });
        var results = await provider.ToSafeMotherboardDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_MotherboardDevice", new[] { BoardRow() });
        var results = await provider.ToSafeMotherboardDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2021, 11, 20, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_MotherboardDevice", WmiRow.Empty());
        var results = await provider.ToSafeMotherboardDeviceMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeMotherboardDeviceMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Board Without Bus Info")));

        var provider = new FakeWmiProvider("Win32_MotherboardDevice", new[] { partial });
        var results = await provider.ToSafeMotherboardDeviceMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Board Without Bus Info", results[0].Name);
        Assert.Null(results[0].PrimaryBusType);
        Assert.Null(results[0].SecondaryBusType);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // Availability stored as a string instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("Availability", new WmiValue("3")));

        var provider = new FakeWmiProvider("Win32_MotherboardDevice", new[] { badRow });
        var results = await provider.ToSafeMotherboardDeviceMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Availability);
    }
}
