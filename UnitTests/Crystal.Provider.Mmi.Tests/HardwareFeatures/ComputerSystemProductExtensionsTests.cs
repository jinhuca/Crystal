using Crystal.Provider.Mmi.HardwareFeatures.ComputerSystemProduct;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class ComputerSystemProductExtensionsTests
{
    [Fact]
    public async Task FullData_Maps_Identity()
    {
        var row = WmiRow.Build(("Name", new WmiValue("Precision 5690")), ("Vendor", new WmiValue("Dell Inc.")), ("Version", new WmiValue("1.0")), ("UUID", new WmiValue("1234")), ("IdentifyingNumber", new WmiValue("ABC123")), ("SKUNumber", new WmiValue("SKU-1")));
        var provider = new FakeWmiProvider("Win32_ComputerSystemProduct", new[] { row });
        var result = await provider.ToSafeComputerSystemProductMetricsAsync(CancellationToken.None);
        Assert.Equal("Precision 5690", result.Name);
        Assert.Equal("Dell Inc.", result.Vendor);
        Assert.Equal("1234", result.UUID);
    }
}
