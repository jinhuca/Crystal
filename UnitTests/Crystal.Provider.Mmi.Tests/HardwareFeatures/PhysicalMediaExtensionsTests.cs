using Crystal.Provider.Mmi.HardwareFeatures.PhysicalMedia;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class PhysicalMediaExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> DiskMediaRow() => WmiRow.Build(
        ("Capacity", new WmiValue(2000398934016UL)),
        ("Caption", new WmiValue("\\\\.\\PHYSICALDRIVE0")),
        ("CleanerMedia", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_PhysicalMedia")),
        ("Description", new WmiValue("\\\\.\\PHYSICALDRIVE0")),
        ("HotSwappable", new WmiValue(false)),
        ("InstallDate", new WmiValue(new DateTime(2023, 4, 18, 0, 0, 0, DateTimeKind.Utc))),
        ("Manufacturer", new WmiValue("(Standard disk drives)")),
        ("MediaDescription", new WmiValue("Fixed hard disk")),
        ("MediaType", new WmiValue(12)),
        ("Model", new WmiValue("Samsung SSD 970 EVO")),
        ("Name", new WmiValue("\\\\.\\PHYSICALDRIVE0")),
        ("OtherIdentifyingInfo", new WmiValue("")),
        ("PartNumber", new WmiValue("")),
        ("PoweredOn", new WmiValue(true)),
        ("Removable", new WmiValue(false)),
        ("Replaceable", new WmiValue(true)),
        ("SerialNumber", new WmiValue("WD-WCC4M1DCUPP1")),
        ("SKU", new WmiValue("")),
        ("Status", new WmiValue("OK")),
        ("Tag", new WmiValue("\\\\.\\PHYSICALDRIVE0")),
        ("Version", new WmiValue("")),
        ("WriteProtectOn", new WmiValue(false))
    );

    [Fact]
    public async Task FullData_Maps_Tag()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMedia", new[] { DiskMediaRow() });
        var results = await provider.ToSafePhysicalMediaMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("\\\\.\\PHYSICALDRIVE0", results[0].Tag);
    }

    [Fact]
    public async Task FullData_Maps_SerialNumber()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMedia", new[] { DiskMediaRow() });
        var results = await provider.ToSafePhysicalMediaMetricsAsync(CancellationToken.None);

        Assert.Equal("WD-WCC4M1DCUPP1", results[0].SerialNumber);
    }

    [Fact]
    public async Task FullData_Maps_Capacity_Ulong()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMedia", new[] { DiskMediaRow() });
        var results = await provider.ToSafePhysicalMediaMetricsAsync(CancellationToken.None);

        Assert.Equal(2000398934016UL, results[0].Capacity);
    }

    [Fact]
    public async Task FullData_Maps_Model()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMedia", new[] { DiskMediaRow() });
        var results = await provider.ToSafePhysicalMediaMetricsAsync(CancellationToken.None);

        Assert.Equal("Samsung SSD 970 EVO", results[0].Model);
    }

    [Fact]
    public async Task FullData_Maps_Removable_False()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMedia", new[] { DiskMediaRow() });
        var results = await provider.ToSafePhysicalMediaMetricsAsync(CancellationToken.None);

        Assert.False(results[0].Removable);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMedia", new[] { DiskMediaRow() });
        var results = await provider.ToSafePhysicalMediaMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMedia", new[] { DiskMediaRow() });
        var results = await provider.ToSafePhysicalMediaMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2023, 4, 18, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMedia", WmiRow.Empty());
        var results = await provider.ToSafePhysicalMediaMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafePhysicalMediaMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleMedia_Returns_All()
    {
        var media1 = WmiRow.Build(("Tag", new WmiValue("\\\\.\\PHYSICALDRIVE0")));
        var media2 = WmiRow.Build(("Tag", new WmiValue("\\\\.\\PHYSICALDRIVE1")));

        var provider = new FakeWmiProvider("Win32_PhysicalMedia", new[] { media1, media2 });
        var results = await provider.ToSafePhysicalMediaMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("\\\\.\\PHYSICALDRIVE0", results[0].Tag);
        Assert.Equal("\\\\.\\PHYSICALDRIVE1", results[1].Tag);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Tag", new WmiValue("\\\\.\\PHYSICALDRIVE2")));

        var provider = new FakeWmiProvider("Win32_PhysicalMedia", new[] { partial });
        var results = await provider.ToSafePhysicalMediaMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("\\\\.\\PHYSICALDRIVE2", results[0].Tag);
        Assert.Null(results[0].Capacity);
        Assert.Null(results[0].SerialNumber);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // Capacity stored as an Int instead of ULong — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("Capacity", new WmiValue(123456)));

        var provider = new FakeWmiProvider("Win32_PhysicalMedia", new[] { badRow });
        var results = await provider.ToSafePhysicalMediaMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Capacity);
    }
}
