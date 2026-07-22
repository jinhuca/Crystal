using System.Collections.Frozen;
using Crystal.Mmi.HardwareFeatures.DiskDrive;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class DiskDriveExtensionsTests
{
    private static FrozenDictionary<string, WmiValue> SamsungDisk() => WmiRow.Build(
        ("Model", new WmiValue("Samsung SSD 870 EVO")),
        ("SerialNumber", new WmiValue("SN-ABCDEF")),
        ("DeviceID", new WmiValue("\\\\.\\PHYSICALDRIVE0")),
        ("InterfaceType", new WmiValue("SATA")),
        ("MediaType", new WmiValue("Fixed hard disk media")),
        ("Size", new WmiValue(500_107_862_016UL)),
        ("Partitions", new WmiValue(3)),
        ("Index", new WmiValue(0)),
        ("BytesPerSector", new WmiValue(512)),
        ("SectorsPerTrack", new WmiValue(63)),
        ("TotalCylinders", new WmiValue(60_801UL)),
        ("TotalHeads", new WmiValue(255)),
        ("TotalSectors", new WmiValue(976_773_168UL)),
        ("TotalTracks", new WmiValue(15_504_255UL)),
        ("TracksPerCylinder", new WmiValue(255)),
        ("MediaLoaded", new WmiValue(true)),
        ("Status", new WmiValue("OK")),
        ("Capabilities", new WmiValue(new ushort[] { 3, 4 })),
        ("CapabilityDescriptions", new WmiValue(new[] { "Random Access", "Supports Writing" })),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("PowerManagementSupported", new WmiValue(false))
    );

    [Fact]
    public async Task FullData_Maps_Model()
    {
        var provider = new FakeWmiProvider("Win32_DiskDrive", new[] { SamsungDisk() });
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Samsung SSD 870 EVO", results[0].Model);
    }

    [Fact]
    public async Task FullData_Maps_SerialNumber()
    {
        var provider = new FakeWmiProvider("Win32_DiskDrive", new[] { SamsungDisk() });
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.Equal("SN-ABCDEF", results[0].SerialNumber);
    }

    [Fact]
    public async Task FullData_Maps_Size_As_ULong()
    {
        var provider = new FakeWmiProvider("Win32_DiskDrive", new[] { SamsungDisk() });
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(500_107_862_016UL, results[0].Size);
    }

    [Fact]
    public async Task FullData_Maps_Partitions_As_Uint()
    {
        var provider = new FakeWmiProvider("Win32_DiskDrive", new[] { SamsungDisk() });
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)3, results[0].Partitions);
    }

    [Fact]
    public async Task FullData_Maps_MediaLoaded_True()
    {
        var provider = new FakeWmiProvider("Win32_DiskDrive", new[] { SamsungDisk() });
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.True(results[0].MediaLoaded);
    }

    [Fact]
    public async Task FullData_Maps_Capabilities_UShortArray()
    {
        var provider = new FakeWmiProvider("Win32_DiskDrive", new[] { SamsungDisk() });
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(new ushort[] { 3, 4 }, results[0].Capabilities);
    }

    [Fact]
    public async Task FullData_Maps_CapabilityDescriptions_StringArray()
    {
        var provider = new FakeWmiProvider("Win32_DiskDrive", new[] { SamsungDisk() });
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(new[] { "Random Access", "Supports Writing" }, results[0].CapabilityDescriptions);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_DiskDrive", new[] { SamsungDisk() });
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_InterfaceType()
    {
        var provider = new FakeWmiProvider("Win32_DiskDrive", new[] { SamsungDisk() });
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.Equal("SATA", results[0].InterfaceType);
    }

    [Fact]
    public async Task FullData_Maps_BytesPerSector_Uint()
    {
        var provider = new FakeWmiProvider("Win32_DiskDrive", new[] { SamsungDisk() });
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)512, results[0].BytesPerSector);
    }

    [Fact]
    public async Task FullData_Maps_TotalCylinders_ULong()
    {
        var provider = new FakeWmiProvider("Win32_DiskDrive", new[] { SamsungDisk() });
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(60_801UL, results[0].TotalCylinders);
    }

    [Fact]
    public async Task FullData_Maps_TotalSectors_ULong()
    {
        var provider = new FakeWmiProvider("Win32_DiskDrive", new[] { SamsungDisk() });
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(976_773_168UL, results[0].TotalSectors);
    }

    [Fact]
    public async Task MultipleDisks_Returns_All()
    {
        var disk1 = WmiRow.Build(("Model", new WmiValue("Disk1")), ("Size", new WmiValue(100UL)));
        var disk2 = WmiRow.Build(("Model", new WmiValue("Disk2")), ("Size", new WmiValue(200UL)));
        var disk3 = WmiRow.Build(("Model", new WmiValue("Disk3")), ("Size", new WmiValue(300UL)));

        var provider = new FakeWmiProvider("Win32_DiskDrive", new[] { disk1, disk2, disk3 });
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(3, results.Count);
        Assert.Equal("Disk1", results[0].Model);
        Assert.Equal("Disk2", results[1].Model);
        Assert.Equal("Disk3", results[2].Model);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_DiskDrive", WmiRow.Empty());
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task UnknownClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider(
            new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>());
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingKeys_Returns_Null_Fields()
    {
        var provider = new FakeWmiProvider("Win32_DiskDrive",
            WmiRow.Single(("Model", new WmiValue("OnlyModel"))));
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("OnlyModel", results[0].Model);
        Assert.Null(results[0].SerialNumber);
        Assert.Null(results[0].Size);
        Assert.Null(results[0].Capabilities);
    }

    [Fact]
    public async Task PowerManagementCapabilities_Maps_UShortArray()
    {
        var row = WmiRow.Build(
            ("Model", new WmiValue("Drive")),
            ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1, 6 })));
        var provider = new FakeWmiProvider("Win32_DiskDrive", new[] { row });
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.Equal(new ushort[] { 1, 6 }, results[0].PowerManagementCapabilities);
    }

    [Fact]
    public async Task ConfigManagerUserConfig_False_Maps_Correctly()
    {
        var provider = new FakeWmiProvider("Win32_DiskDrive", new[] { SamsungDisk() });
        var results = await provider.ToSafeDiskDriveMetricsAsync(CancellationToken.None);

        Assert.False(results[0].ConfigManagerUserConfig);
    }
}
