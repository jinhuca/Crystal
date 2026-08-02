using Crystal.Provider.Mmi.HardwareFeatures.PhysicalMemoryArray;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class PhysicalMemoryArrayExtensionsTests
{
    [Fact]
    public async Task FullData_Maps_Core_And_Computed_Fields()
    {
        var row = WmiRow.Build(("Caption", new WmiValue("Physical Memory Array")), ("Location", new WmiValue(3)), ("Use", new WmiValue(3)), ("MemoryDevices", new WmiValue(4)), ("MemoryErrorCorrection", new WmiValue(6)), ("MaxCapacityEx", new WmiValue(137438953472UL)), ("Status", new WmiValue("OK")));
        var provider = new FakeWmiProvider("Win32_PhysicalMemoryArray", new[] { row });
        var result = (await provider.ToSafePhysicalMemoryArrayMetricsAsync(CancellationToken.None))[0];
        Assert.Equal("Physical Memory Array", result.Caption);
        Assert.Equal("System Board or Motherboard", result.LocationName);
        Assert.Equal("System Memory", result.UseName);
        Assert.Equal("Multi-bit ECC", result.MemoryErrorCorrectionName);
        Assert.Equal(128, result.MaxCapacityExInGB);
    }
}
