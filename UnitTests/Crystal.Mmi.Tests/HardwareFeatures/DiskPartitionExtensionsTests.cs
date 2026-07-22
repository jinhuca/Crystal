using System.Collections.Frozen;
using Crystal.Mmi.HardwareFeatures.DiskPartition;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class DiskPartitionExtensionsTests
{
    private static FrozenDictionary<string, WmiValue> PartitionRow(
        string deviceId = "Disk #0, Partition #0",
        ulong size = 536_870_912_000UL) => WmiRow.Build(
        ("DeviceID", new WmiValue(deviceId)),
        ("Caption", new WmiValue(deviceId)),
        ("Name", new WmiValue(deviceId)),
        ("Description", new WmiValue("Installable File System")),
        ("DiskIndex", new WmiValue(0)),
        ("Index", new WmiValue(0)),
        ("Size", new WmiValue(size)),
        ("StartingOffset", new WmiValue(1_048_576UL)),
        ("NumberOfBlocks", new WmiValue(1_048_576UL)),
        ("BlockSize", new WmiValue(512UL)),
        ("Bootable", new WmiValue(true)),
        ("BootPartition", new WmiValue(true)),
        ("PrimaryPartition", new WmiValue(true)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("Type", new WmiValue(12)),
        ("Availability", new WmiValue(6)),
        ("PowerManagementSupported", new WmiValue(false)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("ErrorCleared", new WmiValue(false)),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 }))
    );

    // ── ToSafeDiskPartitionMetricsAsync ────────────────────────────────────

    [Fact]
    public async Task FullData_Maps_DeviceID()
    {
        var provider = new FakeWmiProvider("Win32_DiskPartition", new[] { PartitionRow() });
        var results = await provider.ToSafeDiskPartitionMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Disk #0, Partition #0", results[0].DeviceID);
    }

    [Fact]
    public async Task FullData_Maps_Size_ULong()
    {
        var provider = new FakeWmiProvider("Win32_DiskPartition", new[] { PartitionRow() });
        var results = await provider.ToSafeDiskPartitionMetricsAsync(CancellationToken.None);

        Assert.Equal(536_870_912_000UL, results[0].Size);
    }

    [Fact]
    public async Task FullData_Maps_Bootable_True()
    {
        var provider = new FakeWmiProvider("Win32_DiskPartition", new[] { PartitionRow() });
        var results = await provider.ToSafeDiskPartitionMetricsAsync(CancellationToken.None);

        Assert.True(results[0].Bootable);
    }

    [Fact]
    public async Task FullData_Maps_BootPartition_True()
    {
        var provider = new FakeWmiProvider("Win32_DiskPartition", new[] { PartitionRow() });
        var results = await provider.ToSafeDiskPartitionMetricsAsync(CancellationToken.None);

        Assert.True(results[0].BootPartition);
    }

    [Fact]
    public async Task FullData_Maps_PrimaryPartition_True()
    {
        var provider = new FakeWmiProvider("Win32_DiskPartition", new[] { PartitionRow() });
        var results = await provider.ToSafeDiskPartitionMetricsAsync(CancellationToken.None);

        Assert.True(results[0].PrimaryPartition);
    }

    [Fact]
    public async Task FullData_Maps_DiskIndex_Uint()
    {
        var provider = new FakeWmiProvider("Win32_DiskPartition", new[] { PartitionRow() });
        var results = await provider.ToSafeDiskPartitionMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)0, results[0].DiskIndex);
    }

    [Fact]
    public async Task FullData_Maps_StartingOffset_ULong()
    {
        var provider = new FakeWmiProvider("Win32_DiskPartition", new[] { PartitionRow() });
        var results = await provider.ToSafeDiskPartitionMetricsAsync(CancellationToken.None);

        Assert.Equal(1_048_576UL, results[0].StartingOffset);
    }

    [Fact]
    public async Task MultiplePartitions_Returns_All()
    {
        var p0 = PartitionRow("Disk #0, Partition #0");
        var p1 = PartitionRow("Disk #0, Partition #1");

        var provider = new FakeWmiProvider("Win32_DiskPartition", new[] { p0, p1 });
        var results = await provider.ToSafeDiskPartitionMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Disk #0, Partition #0", results[0].DeviceID);
        Assert.Equal("Disk #0, Partition #1", results[1].DeviceID);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_DiskPartition", WmiRow.Empty());
        var results = await provider.ToSafeDiskPartitionMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    // ── ToResolvedDriveTopologyAsync ───────────────────────────────────────

    [Fact]
    public async Task ResolvedTopology_Maps_Drive_To_Partitions()
    {
        // Use a simple DeviceID without backslashes to avoid the \\→\ Replace logic complication.
        // The bridge antecedent is parsed via Split("DeviceID=\"")+TrimEnd('"')+Replace("\\","\"),
        // so the driveId after parsing must equal the drive's DeviceID exactly.
        // With no backslashes the Replace is a no-op and matching is straightforward.
        const string driveId = "PHYSICALDRIVE0";
        const string partId  = "Disk #0, Partition #0";

        var drive = WmiRow.Build(
            ("DeviceID", new WmiValue(driveId)),
            ("Model", new WmiValue("Samsung SSD")));

        var partition = PartitionRow(partId);

        // Bridge: Win32_DiskDriveToDiskPartition — antecedent encodes the drive DeviceID
        var driveToPart = WmiRow.Build(
            ("Antecedent", new WmiValue($"Win32_DiskDrive.DeviceID=\"{driveId}\"")),
            ("Dependent",  new WmiValue($"Win32_DiskPartition.DeviceID=\"{partId}\"")));

        // Bridge: Win32_LogicalDiskToPartition — antecedent encodes the partition DeviceID
        var partToLogical = WmiRow.Build(
            ("Antecedent", new WmiValue($"Win32_DiskPartition.DeviceID=\"{partId}\"")),
            ("Dependent",  new WmiValue("Win32_LogicalDisk.DeviceID=\"C:\"")));

        var provider = new FakeWmiProvider(new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>
        {
            ["Win32_DiskDrive"]               = new[] { drive },
            ["Win32_DiskPartition"]           = new[] { partition },
            ["Win32_DiskDriveToDiskPartition"]= new[] { driveToPart },
            ["Win32_LogicalDiskToPartition"]  = new[] { partToLogical }
        });

        var topology = await provider.ToResolvedDriveTopologyAsync(CancellationToken.None);

        Assert.Single(topology);
        Assert.Equal("Samsung SSD", topology[0].DriveInfo.Model);
        Assert.Single(topology[0].Partitions);
        Assert.Equal(partId, topology[0].Partitions[0].PartitionInfo.DeviceID);
        Assert.Single(topology[0].Partitions[0].VolumeLetters);
        Assert.Equal("C:", topology[0].Partitions[0].VolumeLetters[0]);
    }

    [Fact]
    public async Task ResolvedTopology_Drive_Without_Partitions_Has_Empty_Partitions()
    {
        var drive = WmiRow.Build(
            ("DeviceID", new WmiValue("PHYSICALDRIVE0")),
            ("Model", new WmiValue("Unpartitioned Drive")));

        var provider = new FakeWmiProvider(new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>
        {
            ["Win32_DiskDrive"]               = new[] { drive },
            ["Win32_DiskPartition"]           = WmiRow.Empty(),
            ["Win32_DiskDriveToDiskPartition"]= WmiRow.Empty(),
            ["Win32_LogicalDiskToPartition"]  = WmiRow.Empty()
        });

        var topology = await provider.ToResolvedDriveTopologyAsync(CancellationToken.None);

        Assert.Single(topology);
        Assert.Empty(topology[0].Partitions);
    }

    [Fact]
    public async Task ResolvedTopology_NoDrives_Returns_Empty()
    {
        var provider = new FakeWmiProvider(new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>
        {
            ["Win32_DiskDrive"] = WmiRow.Empty(),
            ["Win32_DiskPartition"] = WmiRow.Empty(),
            ["Win32_DiskDriveToDiskPartition"] = WmiRow.Empty(),
            ["Win32_LogicalDiskToPartition"] = WmiRow.Empty()
        });

        var topology = await provider.ToResolvedDriveTopologyAsync(CancellationToken.None);

        Assert.Empty(topology);
    }

    [Fact]
    public async Task ResolvedTopology_Partition_With_No_VolumeLetters_Has_Empty_Letters()
    {
        const string driveId = "PHYSICALDRIVE0";
        const string partId  = "Disk #0, Partition #0";

        var drive = WmiRow.Build(
            ("DeviceID", new WmiValue(driveId)),
            ("Model", new WmiValue("Drive")));

        var partition = PartitionRow(partId);

        var driveToPart = WmiRow.Build(
            ("Antecedent", new WmiValue($"Win32_DiskDrive.DeviceID=\"{driveId}\"")),
            ("Dependent",  new WmiValue($"Win32_DiskPartition.DeviceID=\"{partId}\"")));

        // No Win32_LogicalDiskToPartition entries — partition has no drive letters
        var provider = new FakeWmiProvider(new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>
        {
            ["Win32_DiskDrive"]               = new[] { drive },
            ["Win32_DiskPartition"]           = new[] { partition },
            ["Win32_DiskDriveToDiskPartition"]= new[] { driveToPart },
            ["Win32_LogicalDiskToPartition"]  = WmiRow.Empty()
        });

        var topology = await provider.ToResolvedDriveTopologyAsync(CancellationToken.None);

        Assert.Single(topology[0].Partitions);
        Assert.Empty(topology[0].Partitions[0].VolumeLetters);
    }

    [Fact]
    public void ResolvedPartition_Record_Contains_PartitionInfo_And_Letters()
    {
        var partInfo = new Crystal.Mmi.HardwareFeatures.DiskPartition.DiskPartitionMetrics(
            null, null, null, null, null, null, null, null, null, "MyPart",
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null, null);
        var letters = new[] { "D:", "E:" };

        var rp = new ResolvedPartition(partInfo, letters);

        Assert.Equal("MyPart", rp.PartitionInfo.DeviceID);
        Assert.Equal(new[] { "D:", "E:" }, rp.VolumeLetters);
    }
}
