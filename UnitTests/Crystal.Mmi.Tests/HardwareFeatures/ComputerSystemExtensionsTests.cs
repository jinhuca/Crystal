using Crystal.Mmi.HardwareFeatures.ComputerSystem;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class ComputerSystemExtensionsTests
{
    [Fact]
    public async Task FullData_Maps_Core_Fields()
    {
        var row = WmiRow.Build(("Name", new WmiValue("DESKTOP-01")), ("Manufacturer", new WmiValue("Dell Inc.")), ("Model", new WmiValue("Precision 5690")), ("TotalPhysicalMemory", new WmiValue(68719476736UL)), ("NumberOfProcessors", new WmiValue(1)), ("NumberOfLogicalProcessors", new WmiValue(22)), ("HypervisorPresent", new WmiValue(true)), ("Status", new WmiValue("OK")));
        var provider = new FakeWmiProvider("Win32_ComputerSystem", new[] { row });
        var result = await provider.ToSafeComputerSystemMetricsAsync(CancellationToken.None);
        Assert.Equal("DESKTOP-01", result.Name);
        Assert.Equal(68719476736UL, result.TotalPhysicalMemory);
        Assert.True(result.HypervisorPresent);
    }
}
