using Crystal.Provider.Mmi.HardwareFeatures.TapeDrive;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class TapeDriveExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> TapeDriveRow() => WmiRow.Build(
        ("Name", new WmiValue("Tape Drive 0")),
        ("Caption", new WmiValue("HP Ultrium")),
        ("Description", new WmiValue("Tape Drive")),
        ("DeviceID", new WmiValue("TAPE0")),
        ("PNPDeviceID", new WmiValue("SCSI\\SEQUENTIAL")),
        ("Manufacturer", new WmiValue("HP")),
        ("MediaType", new WmiValue("LTO")),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("Availability", new WmiValue(3)),
        ("Compression", new WmiValue(1)),
        ("NeedsCleaning", new WmiValue(false)),
        ("PowerManagementSupported", new WmiValue(true)),
        ("DefaultBlockSize", new WmiValue(65536UL)),
        ("MaxBlockSize", new WmiValue(131072UL)),
        ("MinBlockSize", new WmiValue(512UL)),
        ("Capabilities", new WmiValue(new ushort[] { 1, 2 })),
        ("CapabilityDescriptions", new WmiValue(new[] { "a" })),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("CreationClassName", new WmiValue("Win32_TapeDrive")),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("InstallDate", new WmiValue(new DateTime(2020, 5, 1, 0, 0, 0, DateTimeKind.Utc)))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_TapeDrive", new[] { TapeDriveRow() });
        var results = await provider.ToSafeTapeDriveMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Tape Drive 0", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_Manufacturer()
    {
        var provider = new FakeWmiProvider("Win32_TapeDrive", new[] { TapeDriveRow() });
        var results = await provider.ToSafeTapeDriveMetricsAsync(CancellationToken.None);

        Assert.Equal("HP", results[0].Manufacturer);
    }

    [Fact]
    public async Task FullData_Maps_NeedsCleaning_Bool()
    {
        var provider = new FakeWmiProvider("Win32_TapeDrive", new[] { TapeDriveRow() });
        var results = await provider.ToSafeTapeDriveMetricsAsync(CancellationToken.None);

        Assert.False(results[0].NeedsCleaning);
    }

    [Fact]
    public async Task FullData_Maps_PowerManagementSupported_True()
    {
        var provider = new FakeWmiProvider("Win32_TapeDrive", new[] { TapeDriveRow() });
        var results = await provider.ToSafeTapeDriveMetricsAsync(CancellationToken.None);

        Assert.True(results[0].PowerManagementSupported);
    }

    [Fact]
    public async Task FullData_Maps_DefaultBlockSize_Ulong()
    {
        var provider = new FakeWmiProvider("Win32_TapeDrive", new[] { TapeDriveRow() });
        var results = await provider.ToSafeTapeDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(65536UL, results[0].DefaultBlockSize);
    }

    [Fact]
    public async Task FullData_Maps_Compression_Uint()
    {
        var provider = new FakeWmiProvider("Win32_TapeDrive", new[] { TapeDriveRow() });
        var results = await provider.ToSafeTapeDriveMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)1, results[0].Compression);
    }

    [Fact]
    public async Task FullData_Maps_Availability_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_TapeDrive", new[] { TapeDriveRow() });
        var results = await provider.ToSafeTapeDriveMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].Availability);
    }

    [Fact]
    public async Task FullData_Maps_StatusInfo_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_TapeDrive", new[] { TapeDriveRow() });
        var results = await provider.ToSafeTapeDriveMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].StatusInfo);
    }

    [Fact]
    public async Task FullData_Maps_Capabilities_UshortArray()
    {
        var provider = new FakeWmiProvider("Win32_TapeDrive", new[] { TapeDriveRow() });
        var results = await provider.ToSafeTapeDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(new ushort[] { 1, 2 }, results[0].Capabilities);
    }

    [Fact]
    public async Task FullData_Maps_CapabilityDescriptions_StringArray()
    {
        var provider = new FakeWmiProvider("Win32_TapeDrive", new[] { TapeDriveRow() });
        var results = await provider.ToSafeTapeDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(new[] { "a" }, results[0].CapabilityDescriptions);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate_DateTime()
    {
        var provider = new FakeWmiProvider("Win32_TapeDrive", new[] { TapeDriveRow() });
        var results = await provider.ToSafeTapeDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2020, 5, 1, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task MultipleDrives_Returns_All()
    {
        var d1 = WmiRow.Build(("Name", new WmiValue("Tape0")), ("DeviceID", new WmiValue("TAPE0")));
        var d2 = WmiRow.Build(("Name", new WmiValue("Tape1")), ("DeviceID", new WmiValue("TAPE1")));
        var d3 = WmiRow.Build(("Name", new WmiValue("Tape2")), ("DeviceID", new WmiValue("TAPE2")));

        var provider = new FakeWmiProvider("Win32_TapeDrive", new[] { d1, d2, d3 });
        var results = await provider.ToSafeTapeDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(3, results.Count);
        Assert.Equal("Tape0", results[0].Name);
        Assert.Equal("Tape1", results[1].Name);
        Assert.Equal("Tape2", results[2].Name);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_TapeDrive", WmiRow.Empty());
        var results = await provider.ToSafeTapeDriveMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingKeys_Return_Null_Fields()
    {
        var row = WmiRow.Build(
            ("Name", new WmiValue("minimal")),
            ("NeedsCleaning", new WmiValue(false)));
        var provider = new FakeWmiProvider("Win32_TapeDrive", new[] { row });
        var results = await provider.ToSafeTapeDriveMetricsAsync(CancellationToken.None);

        Assert.Equal("minimal", results[0].Name);
        Assert.False(results[0].NeedsCleaning);
        Assert.Null(results[0].Manufacturer);
        Assert.Null(results[0].MediaType);
        Assert.Null(results[0].DefaultBlockSize);
        Assert.Null(results[0].Capabilities);
        Assert.Null(results[0].Availability);
    }
}
