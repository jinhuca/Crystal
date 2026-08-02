using Crystal.Provider.Mmi.HardwareFeatures.SystemSlot;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class SystemSlotExtensionsTests
{
    [Fact]
    public async Task FullData_Maps_Core_And_Computed_Fields()
    {
        var row = WmiRow.Build(("SlotDesignation", new WmiValue("PCIEX16_1")), ("Name", new WmiValue("PCI Express x16 Slot")), ("CurrentUsage", new WmiValue(4)), ("MaxDataWidth", new WmiValue(6)), ("Manufacturer", new WmiValue("ASUSTeK")), ("PMESignal", new WmiValue(true)), ("SupportsHotPlug", new WmiValue(false)), ("Status", new WmiValue("OK")));
        var provider = new FakeWmiProvider("Win32_SystemSlot", new[] { row });
        var result = (await provider.ToSafeSystemSlotMetricsAsync(CancellationToken.None))[0];
        Assert.Equal("PCIEX16_1", result.SlotDesignation);
        Assert.Equal("In Use", result.CurrentUsageName);
        Assert.Equal("64-bit", result.SlotWidthName);
        Assert.True(result.PMESignal);
    }
}
