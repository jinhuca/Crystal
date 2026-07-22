using Crystal.Mmi.HardwareFeatures.PhysicalMemory;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class PhysicalMemoryExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> DimmRow() => WmiRow.Build(
        ("BankLabel", new WmiValue("BANK 0")),
        ("Capacity", new WmiValue(8_589_934_592UL)),   // 8 GB
        ("Caption", new WmiValue("Physical Memory")),
        ("ConfiguredClockSpeed", new WmiValue(3200)),
        ("ConfiguredVoltage", new WmiValue(1200)),
        ("DeviceLocator", new WmiValue("DIMM_A1")),
        ("FormFactor", new WmiValue(8)),                // DIMM (Desktop)
        ("Manufacturer", new WmiValue("Corsair")),
        ("PartNumber", new WmiValue("CMK16GX4M2B3200C16")),
        ("SerialNumber", new WmiValue("SN-RAM-01")),
        ("Speed", new WmiValue(3200)),
        ("Tag", new WmiValue("Physical Memory 0")),
        ("TotalWidth", new WmiValue(64)),
        ("DataWidth", new WmiValue(64)),
        ("TypeDetail", new WmiValue(128)),              // Synchronous
        ("MemoryType", new WmiValue(26)),
        ("HotSwappable", new WmiValue(false)),
        ("PoweredOn", new WmiValue(true)),
        ("Removable", new WmiValue(true)),
        ("Replaceable", new WmiValue(true)),
        ("Status", new WmiValue("OK")),
        ("CreationClassName", new WmiValue("Win32_PhysicalMemory"))
    );

    [Fact]
    public async Task FullData_Maps_BankLabel()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMemory", new[] { DimmRow() });
        var results = await provider.ToSafePhysicalMemoryMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("BANK 0", results[0].BankLabel);
    }

    [Fact]
    public async Task FullData_Maps_Capacity_ULong()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMemory", new[] { DimmRow() });
        var results = await provider.ToSafePhysicalMemoryMetricsAsync(CancellationToken.None);

        Assert.Equal(8_589_934_592UL, results[0].Capacity);
    }

    [Fact]
    public async Task FullData_Maps_Manufacturer()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMemory", new[] { DimmRow() });
        var results = await provider.ToSafePhysicalMemoryMetricsAsync(CancellationToken.None);

        Assert.Equal("Corsair", results[0].Manufacturer);
    }

    [Fact]
    public async Task FullData_Maps_FormFactor_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMemory", new[] { DimmRow() });
        var results = await provider.ToSafePhysicalMemoryMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)8, results[0].FormFactor);
    }

    [Fact]
    public async Task FullData_Maps_HotSwappable_False()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMemory", new[] { DimmRow() });
        var results = await provider.ToSafePhysicalMemoryMetricsAsync(CancellationToken.None);

        Assert.False(results[0].HotSwappable);
    }

    [Fact]
    public async Task FullData_Maps_ConfiguredClockSpeed_Uint()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMemory", new[] { DimmRow() });
        var results = await provider.ToSafePhysicalMemoryMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)3200, results[0].ConfiguredClockSpeed);
    }

    [Fact]
    public async Task FullData_Maps_Speed_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMemory", new[] { DimmRow() });
        var results = await provider.ToSafePhysicalMemoryMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3200, results[0].Speed);
    }

    [Fact]
    public async Task FormFactorName_DIMM_Desktop()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMemory", new[] { DimmRow() });
        var results = await provider.ToSafePhysicalMemoryMetricsAsync(CancellationToken.None);

        Assert.Equal("DIMM (Desktop)", results[0].FormFactorName);
    }

    [Fact]
    public async Task FormFactorName_SODIMM_Laptop()
    {
        var row = WmiRow.Build(("FormFactor", new WmiValue(12)));
        var provider = new FakeWmiProvider("Win32_PhysicalMemory", new[] { row });
        var results = await provider.ToSafePhysicalMemoryMetricsAsync(CancellationToken.None);

        Assert.Equal("SODIMM (Laptop)", results[0].FormFactorName);
    }

    [Theory]
    [InlineData(13, "Row of chips")]
    [InlineData(15, "SIMM")]
    [InlineData(99, "Unknown Form Factor")]
    public async Task FormFactorName_Various_Codes(int code, string expected)
    {
        var row = WmiRow.Build(("FormFactor", new WmiValue(code)));
        var provider = new FakeWmiProvider("Win32_PhysicalMemory", new[] { row });
        var results = await provider.ToSafePhysicalMemoryMetricsAsync(CancellationToken.None);

        Assert.Equal(expected, results[0].FormFactorName);
    }

    [Fact]
    public async Task FormFactorName_Null_FormFactor_Returns_Unknown()
    {
        // No FormFactor key → FormFactor is null → switch default
        var row = WmiRow.Build(("BankLabel", new WmiValue("BANK 0")));
        var provider = new FakeWmiProvider("Win32_PhysicalMemory", new[] { row });
        var results = await provider.ToSafePhysicalMemoryMetricsAsync(CancellationToken.None);

        Assert.Equal("Unknown Form Factor", results[0].FormFactorName);
    }

    [Fact]
    public async Task CapacityInGB_8GB_Stick()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMemory", new[] { DimmRow() });
        var results = await provider.ToSafePhysicalMemoryMetricsAsync(CancellationToken.None);

        Assert.NotNull(results[0].CapacityInGB);
        Assert.Equal(8.0, results[0].CapacityInGB!.Value, precision: 1);
    }

    [Fact]
    public async Task CapacityInGB_Null_When_Capacity_Missing()
    {
        var row = WmiRow.Build(("BankLabel", new WmiValue("BANK 0")));
        var provider = new FakeWmiProvider("Win32_PhysicalMemory", new[] { row });
        var results = await provider.ToSafePhysicalMemoryMetricsAsync(CancellationToken.None);

        Assert.Null(results[0].CapacityInGB);
    }

    [Fact]
    public async Task MultipleSticks_Returns_All()
    {
        var dimm0 = WmiRow.Build(("BankLabel", new WmiValue("BANK 0")), ("Capacity", new WmiValue(8_589_934_592UL)));
        var dimm1 = WmiRow.Build(("BankLabel", new WmiValue("BANK 1")), ("Capacity", new WmiValue(8_589_934_592UL)));

        var provider = new FakeWmiProvider("Win32_PhysicalMemory", new[] { dimm0, dimm1 });
        var results = await provider.ToSafePhysicalMemoryMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("BANK 0", results[0].BankLabel);
        Assert.Equal("BANK 1", results[1].BankLabel);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_PhysicalMemory", WmiRow.Empty());
        var results = await provider.ToSafePhysicalMemoryMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }
}
