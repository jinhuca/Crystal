using Crystal.Provider.Mmi.HardwareFeatures.SystemEnclosure;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class SystemEnclosureExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> TowerRow() => WmiRow.Build(
        ("AssetTag", new WmiValue("TAG-001")),
        ("AudibleAlarm", new WmiValue(false)),
        ("BreachDescription", new WmiValue("")),
        ("CableManagementStrategy", new WmiValue(1)),
        ("Caption", new WmiValue("System Enclosure")),
        ("ChassisTypes", new WmiValue(new ushort[] { 7 })),  // Tower
        ("CreationClassName", new WmiValue("Win32_SystemEnclosure")),
        ("Description", new WmiValue("System Enclosure")),
        ("HeatSinkPresent", new WmiValue(true)),
        ("HotSwappable", new WmiValue(false)),
        ("LockPresent", new WmiValue(false)),
        ("SecurityStatus", new WmiValue(3)),
        ("SerialNumber", new WmiValue("SN-CASE-01")),
        ("SMBIOSAssetTag", new WmiValue("TAG-001")),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("Tag", new WmiValue("System Enclosure 0")),
        ("Version", new WmiValue("1.0")),
        ("VisibleAlarm", new WmiValue(false)),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("SecurityBreach", new WmiValue(3))
    );

    [Fact]
    public async Task FullData_Maps_SerialNumber()
    {
        var provider = new FakeWmiProvider("Win32_SystemEnclosure", new[] { TowerRow() });
        var results = await provider.ToSafeSystemEnclosureMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("SN-CASE-01", results[0].SerialNumber);
    }

    [Fact]
    public async Task FullData_Maps_ChassisTypes_Array()
    {
        var provider = new FakeWmiProvider("Win32_SystemEnclosure", new[] { TowerRow() });
        var results = await provider.ToSafeSystemEnclosureMetricsAsync(CancellationToken.None);

        Assert.Equal(new ushort[] { 7 }, results[0].ChassisTypes);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_SystemEnclosure", new[] { TowerRow() });
        var results = await provider.ToSafeSystemEnclosureMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_AudibleAlarm_False()
    {
        var provider = new FakeWmiProvider("Win32_SystemEnclosure", new[] { TowerRow() });
        var results = await provider.ToSafeSystemEnclosureMetricsAsync(CancellationToken.None);

        Assert.False(results[0].AudibleAlarm);
    }

    [Fact]
    public async Task FullData_Maps_SecurityBreach_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_SystemEnclosure", new[] { TowerRow() });
        var results = await provider.ToSafeSystemEnclosureMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].SecurityBreach);
    }

    [Fact]
    public async Task FullData_Maps_SecurityStatus_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_SystemEnclosure", new[] { TowerRow() });
        var results = await provider.ToSafeSystemEnclosureMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].SecurityStatus);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_SystemEnclosure", WmiRow.Empty());
        var results = await provider.ToSafeSystemEnclosureMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleEnclosures_Returns_All()
    {
        var enc1 = WmiRow.Build(("SerialNumber", new WmiValue("SN-1")), ("ChassisTypes", new WmiValue(new ushort[] { 7 })));
        var enc2 = WmiRow.Build(("SerialNumber", new WmiValue("SN-2")), ("ChassisTypes", new WmiValue(new ushort[] { 9 })));

        var provider = new FakeWmiProvider("Win32_SystemEnclosure", new[] { enc1, enc2 });
        var results = await provider.ToSafeSystemEnclosureMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
    }

    // ── FormFactorName ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(new ushort[] { 1 }, "Other / Custom Enclosure")]
    [InlineData(new ushort[] { 3 }, "Desktop")]
    [InlineData(new ushort[] { 7 }, "Tower")]
    [InlineData(new ushort[] { 9 }, "Laptop")]
    [InlineData(new ushort[] { 10 }, "Notebook")]
    [InlineData(new ushort[] { 13 }, "All in One PC")]
    [InlineData(new ushort[] { 17 }, "Main System Chassis")]
    [InlineData(new ushort[] { 23 }, "Rack Mount Chassis (Server)")]
    [InlineData(new ushort[] { 24 }, "Main Server Blade")]
    public void FormFactorName_Maps_KnownCodes(ushort[] types, string expected)
    {
        var m = new SystemEnclosureMetrics(
            null, null, null, null, null, types,
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null);

        Assert.Equal(expected, m.FormFactorName);
    }

    [Fact]
    public void FormFactorName_Unknown_Code_Returns_Undocumented()
    {
        var m = new SystemEnclosureMetrics(
            null, null, null, null, null, new ushort[] { 99 },
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null);

        Assert.Equal("Undocumented Structural Casing", m.FormFactorName);
    }

    [Fact]
    public void FormFactorName_Null_ChassisTypes_Returns_Undocumented()
    {
        var m = new SystemEnclosureMetrics(
            null, null, null, null, null, null,  // ChassisTypes = null
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null);

        Assert.Equal("Undocumented Structural Casing", m.FormFactorName);
    }

    [Fact]
    public void FormFactorName_Empty_ChassisTypes_Uses_Default_Zero_Which_Is_Undocumented()
    {
        // FirstOrDefault on empty array returns 0 — no match in switch → default
        var m = new SystemEnclosureMetrics(
            null, null, null, null, null, Array.Empty<ushort>(),
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null);

        Assert.Equal("Undocumented Structural Casing", m.FormFactorName);
    }

    [Fact]
    public void FormFactorName_MultiElement_ChassisTypes_Uses_First()
    {
        // Multiple chassis types — only first is used
        var m = new SystemEnclosureMetrics(
            null, null, null, null, null, new ushort[] { 9, 7 },
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null);

        Assert.Equal("Laptop", m.FormFactorName);  // type 9
    }
}
