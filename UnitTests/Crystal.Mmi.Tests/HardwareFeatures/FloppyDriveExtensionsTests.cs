using Crystal.Mmi.HardwareFeatures.FloppyDrive;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class FloppyDriveExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> FullRow() => WmiRow.Build(
        ("Availability", new WmiValue(3)),
        ("Capabilities", new WmiValue(new ushort[] { 3, 4 })),
        ("CapabilityDescriptions", new WmiValue(new[] { "Unknown", "Random Access", "Supports Writing" })),
        ("Caption", new WmiValue("Floppy disk drive")),
        ("CompressionMethod", new WmiValue(0)),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_FloppyDrive")),
        ("DefaultBlockSize", new WmiValue((ulong)512)),
        ("Description", new WmiValue("Floppy disk drive")),
        ("DeviceID", new WmiValue("\\\\.\\A:")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("ErrorMethodology", new WmiValue("")),
        ("InstallDate", new WmiValue(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("Manufacturer", new WmiValue("(Standard floppy disk drives)")),
        ("MaxBlockSize", new WmiValue((ulong)512)),
        ("MaxMediaSize", new WmiValue((ulong)1474560)),
        ("MinBlockSize", new WmiValue((ulong)512)),
        ("Name", new WmiValue("A:")),
        ("NeedsCleaning", new WmiValue(false)),
        ("NumberOfMediaSupported", new WmiValue(1)),
        ("PNPDeviceID", new WmiValue("FDC\\GENERIC_FLOPPY_DRIVE\\0")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01"))
    );

    [Fact]
    public async Task FullData_Maps_Name_And_DeviceID()
    {
        var provider = new FakeWmiProvider("Win32_FloppyDrive", new[] { FullRow() });
        var results = await provider.ToSafeFloppyDriveMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("A:", results[0].Name);
        Assert.Equal("\\\\.\\A:", results[0].DeviceID);
    }

    [Fact]
    public async Task FullData_Maps_MaxMediaSize_Ulong()
    {
        var provider = new FakeWmiProvider("Win32_FloppyDrive", new[] { FullRow() });
        var results = await provider.ToSafeFloppyDriveMetricsAsync(CancellationToken.None);

        Assert.Equal((ulong)1474560, results[0].MaxMediaSize);
    }

    [Fact]
    public async Task FullData_Maps_Capabilities_Array()
    {
        var provider = new FakeWmiProvider("Win32_FloppyDrive", new[] { FullRow() });
        var results = await provider.ToSafeFloppyDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(new ushort[] { 3, 4 }, results[0].Capabilities);
    }

    [Fact]
    public async Task FullData_Maps_CapabilityDescriptions_StringArray()
    {
        var provider = new FakeWmiProvider("Win32_FloppyDrive", new[] { FullRow() });
        var results = await provider.ToSafeFloppyDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(new[] { "Unknown", "Random Access", "Supports Writing" }, results[0].CapabilityDescriptions);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_FloppyDrive", WmiRow.Empty());
        var results = await provider.ToSafeFloppyDriveMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeFloppyDriveMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("A:")));

        var provider = new FakeWmiProvider("Win32_FloppyDrive", new[] { partial });
        var results = await provider.ToSafeFloppyDriveMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("A:", results[0].Name);
        Assert.Null(results[0].MaxMediaSize);
        Assert.Null(results[0].Capabilities);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // MaxMediaSize stored as an Int instead of ULong — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("MaxMediaSize", new WmiValue(1474560)));

        var provider = new FakeWmiProvider("Win32_FloppyDrive", new[] { badRow });
        var results = await provider.ToSafeFloppyDriveMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].MaxMediaSize);
    }

    [Fact]
    public async Task MultipleDrives_Returns_All()
    {
        var a = WmiRow.Build(("Name", new WmiValue("A:")));
        var b = WmiRow.Build(("Name", new WmiValue("B:")));

        var provider = new FakeWmiProvider("Win32_FloppyDrive", new[] { a, b });
        var results = await provider.ToSafeFloppyDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("A:", results[0].Name);
        Assert.Equal("B:", results[1].Name);
    }
}
