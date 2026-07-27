using Crystal.Mmi.HardwareFeatures.MappedLogicalDisk;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class MappedLogicalDiskExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> FullRow() => WmiRow.Build(
        ("Access", new WmiValue(0)),
        ("Availability", new WmiValue(0)),
        ("Caption", new WmiValue("Z:")),
        ("Compressed", new WmiValue(false)),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_MappedLogicalDisk")),
        ("Description", new WmiValue("Mapped network drive")),
        ("DeviceID", new WmiValue("Z:")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("FileSystem", new WmiValue("NTFS")),
        ("FreeSpace", new WmiValue((ulong)500_000_000_000)),
        ("InstallDate", new WmiValue(new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("MaximumComponentLength", new WmiValue(255)),
        ("Name", new WmiValue("Z:")),
        ("NumberOfBlocks", new WmiValue((ulong)0)),
        ("PNPDeviceID", new WmiValue("")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("ProviderName", new WmiValue("\\\\fileserver\\shared")),
        ("Purpose", new WmiValue("")),
        ("QuotasDisabled", new WmiValue(true)),
        ("QuotasIncomplete", new WmiValue(false)),
        ("QuotasRebuilding", new WmiValue(false)),
        ("SessionID", new WmiValue("0")),
        ("Size", new WmiValue((ulong)1_000_000_000_000)),
        ("Status", new WmiValue("")),
        ("StatusInfo", new WmiValue(0)),
        ("SupportsDiskQuotas", new WmiValue(false)),
        ("SupportsFileBasedCompression", new WmiValue(true)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("VolumeName", new WmiValue("Shared")),
        ("VolumeSerialNumber", new WmiValue("1A2B3C4D"))
    );

    [Fact]
    public async Task FullData_Maps_DeviceID_And_ProviderName()
    {
        var provider = new FakeWmiProvider("Win32_MappedLogicalDisk", new[] { FullRow() });
        var results = await provider.ToSafeMappedLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Z:", results[0].DeviceID);
        Assert.Equal("\\\\fileserver\\shared", results[0].ProviderName);
    }

    [Fact]
    public async Task FullData_Maps_Size_And_FreeSpace_Ulong()
    {
        var provider = new FakeWmiProvider("Win32_MappedLogicalDisk", new[] { FullRow() });
        var results = await provider.ToSafeMappedLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Equal((ulong)1_000_000_000_000, results[0].Size);
        Assert.Equal((ulong)500_000_000_000, results[0].FreeSpace);
    }

    [Fact]
    public async Task FreeSpacePercentage_Computes_From_Size_And_FreeSpace()
    {
        var provider = new FakeWmiProvider("Win32_MappedLogicalDisk", new[] { FullRow() });
        var results = await provider.ToSafeMappedLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Equal(50.0, results[0].FreeSpacePercentage);
    }

    [Fact]
    public async Task UsedSpace_Computes_From_Size_And_FreeSpace()
    {
        var provider = new FakeWmiProvider("Win32_MappedLogicalDisk", new[] { FullRow() });
        var results = await provider.ToSafeMappedLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Equal((ulong)500_000_000_000, results[0].UsedSpace);
    }

    [Fact]
    public async Task FullData_Maps_QuotasDisabled_Bool()
    {
        var provider = new FakeWmiProvider("Win32_MappedLogicalDisk", new[] { FullRow() });
        var results = await provider.ToSafeMappedLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.True(results[0].QuotasDisabled);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_MappedLogicalDisk", WmiRow.Empty());
        var results = await provider.ToSafeMappedLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeMappedLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null_And_Computed_Properties_Null()
    {
        var partial = WmiRow.Build(("DeviceID", new WmiValue("Y:")));

        var provider = new FakeWmiProvider("Win32_MappedLogicalDisk", new[] { partial });
        var results = await provider.ToSafeMappedLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Y:", results[0].DeviceID);
        Assert.Null(results[0].Size);
        Assert.Null(results[0].FreeSpacePercentage);
        Assert.Null(results[0].UsedSpace);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // Size stored as an Int instead of ULong — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("Size", new WmiValue(1000)));

        var provider = new FakeWmiProvider("Win32_MappedLogicalDisk", new[] { badRow });
        var results = await provider.ToSafeMappedLogicalDiskMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Size);
    }
}
