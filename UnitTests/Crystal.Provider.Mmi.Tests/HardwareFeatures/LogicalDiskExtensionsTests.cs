using Crystal.Provider.Mmi.HardwareFeatures.LogicalDisk;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class LogicalDiskExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> CDriveRow() => WmiRow.Build(
        ("DeviceID", new WmiValue("C:")),
        ("Caption", new WmiValue("C:")),
        ("Name", new WmiValue("C:")),
        ("FileSystem", new WmiValue("NTFS")),
        ("VolumeName", new WmiValue("Windows")),
        ("VolumeSerialNumber", new WmiValue("A1B2C3D4")),
        ("Description", new WmiValue("Local Fixed Disk")),
        ("DriveType", new WmiValue(3)),      // Fixed
        ("Size", new WmiValue(500_000_000_000UL)),
        ("FreeSpace", new WmiValue(200_000_000_000UL)),
        ("BlockSize", new WmiValue(4096UL)),
        ("NumberOfBlocks", new WmiValue(122_070_312UL)),
        ("MaximumComponentLength", new WmiValue(255)),
        ("Availability", new WmiValue(3)),
        ("SupportsDiskQuotas", new WmiValue(true)),
        ("SupportsFileBasedCompression", new WmiValue(true)),
        ("PowerManagementSupported", new WmiValue(false)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("ErrorCleared", new WmiValue(false)),
        ("Status", new WmiValue("OK")),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 }))
    );

    [Fact]
    public async Task FullData_Maps_DeviceID()
    {
        var provider = new FakeWmiProvider("Win32_LogicalDisk", new[] { CDriveRow() });
        var results = await provider.ToSafeLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("C:", results[0].DeviceID);
    }

    [Fact]
    public async Task FullData_Maps_FileSystem()
    {
        var provider = new FakeWmiProvider("Win32_LogicalDisk", new[] { CDriveRow() });
        var results = await provider.ToSafeLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Equal("NTFS", results[0].FileSystem);
    }

    [Fact]
    public async Task FullData_Maps_Size_ULong()
    {
        var provider = new FakeWmiProvider("Win32_LogicalDisk", new[] { CDriveRow() });
        var results = await provider.ToSafeLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Equal(500_000_000_000UL, results[0].Size);
    }

    [Fact]
    public async Task FullData_Maps_FreeSpace_ULong()
    {
        var provider = new FakeWmiProvider("Win32_LogicalDisk", new[] { CDriveRow() });
        var results = await provider.ToSafeLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Equal(200_000_000_000UL, results[0].FreeSpace);
    }

    [Fact]
    public async Task FullData_Maps_DriveType_Uint()
    {
        var provider = new FakeWmiProvider("Win32_LogicalDisk", new[] { CDriveRow() });
        var results = await provider.ToSafeLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)3, results[0].DriveType);
    }

    [Fact]
    public async Task FullData_Maps_VolumeName()
    {
        var provider = new FakeWmiProvider("Win32_LogicalDisk", new[] { CDriveRow() });
        var results = await provider.ToSafeLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Equal("Windows", results[0].VolumeName);
    }

    [Fact]
    public async Task FullData_Maps_SupportsDiskQuotas_True()
    {
        var provider = new FakeWmiProvider("Win32_LogicalDisk", new[] { CDriveRow() });
        var results = await provider.ToSafeLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.True(results[0].SupportsDiskQuotas);
    }

    [Fact]
    public async Task FullData_Maps_PowerManagementCapabilities_Array()
    {
        var provider = new FakeWmiProvider("Win32_LogicalDisk", new[] { CDriveRow() });
        var results = await provider.ToSafeLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Equal(new ushort[] { 1 }, results[0].PowerManagementCapabilities);
    }

    [Fact]
    public async Task Purpose_Is_Always_Null_Since_Not_Mapped()
    {
        // The extension always passes null for Purpose
        var provider = new FakeWmiProvider("Win32_LogicalDisk", new[] { CDriveRow() });
        var results = await provider.ToSafeLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Null(results[0].Purpose);
    }

    [Fact]
    public async Task MultipleVolumes_Returns_All()
    {
        var c = WmiRow.Build(("DeviceID", new WmiValue("C:")), ("Size", new WmiValue(500_000_000_000UL)));
        var d = WmiRow.Build(("DeviceID", new WmiValue("D:")), ("Size", new WmiValue(2_000_000_000_000UL)));

        var provider = new FakeWmiProvider("Win32_LogicalDisk", new[] { c, d });
        var results = await provider.ToSafeLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("C:", results[0].DeviceID);
        Assert.Equal("D:", results[1].DeviceID);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_LogicalDisk", WmiRow.Empty());
        var results = await provider.ToSafeLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task FreeSpacePercentage_Computed_Correctly()
    {
        // Size=500, FreeSpace=200 → 40.00%
        var provider = new FakeWmiProvider("Win32_LogicalDisk", new[] { CDriveRow() });
        var results = await provider.ToSafeLogicalDiskMetricsAsync(CancellationToken.None);

        var pct = results[0].FreeSpacePercentage;
        Assert.NotNull(pct);
        Assert.Equal(40.0, pct!.Value, precision: 2);
    }

    [Fact]
    public async Task FreeSpacePercentage_Null_When_Size_Is_Zero()
    {
        var row = WmiRow.Build(
            ("Size", new WmiValue(0UL)),
            ("FreeSpace", new WmiValue(0UL)));
        var provider = new FakeWmiProvider("Win32_LogicalDisk", new[] { row });
        var results = await provider.ToSafeLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Null(results[0].FreeSpacePercentage);
    }

    [Fact]
    public async Task UsedSpace_Computed_Correctly()
    {
        var provider = new FakeWmiProvider("Win32_LogicalDisk", new[] { CDriveRow() });
        var results = await provider.ToSafeLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Equal(300_000_000_000UL, results[0].UsedSpace);
    }

    [Fact]
    public async Task UsedSpace_Null_When_Size_Missing()
    {
        var row = WmiRow.Build(("FreeSpace", new WmiValue(100_000_000_000UL)));
        var provider = new FakeWmiProvider("Win32_LogicalDisk", new[] { row });
        var results = await provider.ToSafeLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Null(results[0].UsedSpace);
    }

    [Fact]
    public async Task UsedSpace_Null_When_FreeSpace_Missing()
    {
        var row = WmiRow.Build(("Size", new WmiValue(500_000_000_000UL)));
        var provider = new FakeWmiProvider("Win32_LogicalDisk", new[] { row });
        var results = await provider.ToSafeLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Null(results[0].UsedSpace);
    }
}
