using Crystal.Provider.Mmi.HardwareFeatures.CdRomDrive;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class CDROMDriveExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> OpticalDriveRow() => WmiRow.Build(
        ("Name", new WmiValue("HL-DT-ST DVDRAM GH24NSD1")),
        ("Caption", new WmiValue("HL-DT-ST DVDRAM GH24NSD1")),
        ("Description", new WmiValue("CD-ROM Drive")),
        ("Manufacturer", new WmiValue("(Standard CD-ROM drives)")),
        ("Drive", new WmiValue("E:")),
        ("DeviceID", new WmiValue("IDE\\CDROMHL-DT-ST_DVDRAM")),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("Availability", new WmiValue(3)),
        ("MediaLoaded", new WmiValue(true)),
        ("MediaType", new WmiValue("CR-ROM")),
        ("Size", new WmiValue(500000000UL)),
        ("MaxMediaSize", new WmiValue(700000000UL)),
        ("DefaultBlockSize", new WmiValue(2048UL)),
        ("CapabilityDescriptions", new WmiValue(new[] { "Random Access", "Supports Writing" })),
        ("SCSIPort", new WmiValue(1)),
        ("SCSIBus", new WmiValue(0)),
        ("NeedsCleaning", new WmiValue(false)),
        ("MaximumComponentLength", new WmiValue(255)),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1, 3 })),
        ("InstallDate", new WmiValue(new DateTime(2020, 6, 15, 0, 0, 0, DateTimeKind.Utc)))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_CDROMDrive", new[] { OpticalDriveRow() });
        var results = await provider.ToSafeCDROMDriveMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("HL-DT-ST DVDRAM GH24NSD1", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_Manufacturer()
    {
        var provider = new FakeWmiProvider("Win32_CDROMDrive", new[] { OpticalDriveRow() });
        var results = await provider.ToSafeCDROMDriveMetricsAsync(CancellationToken.None);

        Assert.Equal("(Standard CD-ROM drives)", results[0].Manufacturer);
    }

    [Fact]
    public async Task FullData_Maps_Drive()
    {
        var provider = new FakeWmiProvider("Win32_CDROMDrive", new[] { OpticalDriveRow() });
        var results = await provider.ToSafeCDROMDriveMetricsAsync(CancellationToken.None);

        Assert.Equal("E:", results[0].Drive);
    }

    [Fact]
    public async Task FullData_Maps_MediaLoaded_True()
    {
        var provider = new FakeWmiProvider("Win32_CDROMDrive", new[] { OpticalDriveRow() });
        var results = await provider.ToSafeCDROMDriveMetricsAsync(CancellationToken.None);

        Assert.True(results[0].MediaLoaded);
    }

    [Fact]
    public async Task FullData_Maps_NeedsCleaning_False()
    {
        var provider = new FakeWmiProvider("Win32_CDROMDrive", new[] { OpticalDriveRow() });
        var results = await provider.ToSafeCDROMDriveMetricsAsync(CancellationToken.None);

        Assert.False(results[0].NeedsCleaning);
    }

    [Fact]
    public async Task FullData_Maps_Size_ULong()
    {
        var provider = new FakeWmiProvider("Win32_CDROMDrive", new[] { OpticalDriveRow() });
        var results = await provider.ToSafeCDROMDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(500000000UL, results[0].Size);
    }

    [Fact]
    public async Task FullData_Maps_DefaultBlockSize_ULong()
    {
        var provider = new FakeWmiProvider("Win32_CDROMDrive", new[] { OpticalDriveRow() });
        var results = await provider.ToSafeCDROMDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(2048UL, results[0].DefaultBlockSize);
    }

    [Fact]
    public async Task FullData_Maps_CapabilityDescriptions_StringArray()
    {
        var provider = new FakeWmiProvider("Win32_CDROMDrive", new[] { OpticalDriveRow() });
        var results = await provider.ToSafeCDROMDriveMetricsAsync(CancellationToken.None);

        Assert.NotNull(results[0].CapabilityDescriptions);
        Assert.Equal(2, results[0].CapabilityDescriptions!.Length);
        Assert.Equal("Random Access", results[0].CapabilityDescriptions![0]);
    }

    [Fact]
    public async Task FullData_Maps_SCSIPort_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_CDROMDrive", new[] { OpticalDriveRow() });
        var results = await provider.ToSafeCDROMDriveMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)1, results[0].SCSIPort);
    }

    [Fact]
    public async Task FullData_Maps_SCSIBus_Uint()
    {
        var provider = new FakeWmiProvider("Win32_CDROMDrive", new[] { OpticalDriveRow() });
        var results = await provider.ToSafeCDROMDriveMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)0, results[0].SCSIBus);
    }

    [Fact]
    public async Task FullData_Maps_Availability_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_CDROMDrive", new[] { OpticalDriveRow() });
        var results = await provider.ToSafeCDROMDriveMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].Availability);
    }

    [Fact]
    public async Task FullData_Maps_PowerManagementCapabilities_UshortArray()
    {
        var provider = new FakeWmiProvider("Win32_CDROMDrive", new[] { OpticalDriveRow() });
        var results = await provider.ToSafeCDROMDriveMetricsAsync(CancellationToken.None);

        Assert.NotNull(results[0].PowerManagementCapabilities);
        Assert.Equal(2, results[0].PowerManagementCapabilities!.Length);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate_DateTime()
    {
        var provider = new FakeWmiProvider("Win32_CDROMDrive", new[] { OpticalDriveRow() });
        var results = await provider.ToSafeCDROMDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2020, 6, 15, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task MultipleDrives_Returns_All()
    {
        var d1 = WmiRow.Build(("Name", new WmiValue("Drive1")), ("Drive", new WmiValue("E:")));
        var d2 = WmiRow.Build(("Name", new WmiValue("Drive2")), ("Drive", new WmiValue("F:")));
        var d3 = WmiRow.Build(("Name", new WmiValue("Drive3")), ("Drive", new WmiValue("G:")));

        var provider = new FakeWmiProvider("Win32_CDROMDrive", new[] { d1, d2, d3 });
        var results = await provider.ToSafeCDROMDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(3, results.Count);
        Assert.Equal("Drive1", results[0].Name);
        Assert.Equal("Drive2", results[1].Name);
        Assert.Equal("Drive3", results[2].Name);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_CDROMDrive", WmiRow.Empty());
        var results = await provider.ToSafeCDROMDriveMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingKeys_Return_Null_Fields()
    {
        var row = WmiRow.Build(
            ("Name", new WmiValue("minimal")),
            ("MediaLoaded", new WmiValue(false)));
        var provider = new FakeWmiProvider("Win32_CDROMDrive", new[] { row });
        var results = await provider.ToSafeCDROMDriveMetricsAsync(CancellationToken.None);

        Assert.Equal("minimal", results[0].Name);
        Assert.False(results[0].MediaLoaded);
        Assert.Null(results[0].Drive);
        Assert.Null(results[0].Size);
        Assert.Null(results[0].CapabilityDescriptions);
        Assert.Null(results[0].SCSIPort);
        Assert.Null(results[0].Manufacturer);
    }
}
