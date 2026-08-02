using Crystal.Provider.Mmi.Cli;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.Cli;

public sealed class SystemSlotDemoTests
{
    [Fact]
    public async Task DumpSystemSlots_Writes_Header()
    {
        using var console = new TestConsoleWriter();

        await CliDemos.DumpSystemSlotsAsync(DemoProviders.SystemSlot(), console.Writer, TestContext.Current.CancellationToken);

        Assert.Contains("System Slots", console.Output);
    }

    [Fact]
    public async Task DumpSystemSlots_Writes_Core_Values()
    {
        using var console = new TestConsoleWriter();

        await CliDemos.DumpSystemSlotsAsync(DemoProviders.SystemSlot(), console.Writer, TestContext.Current.CancellationToken);

        Assert.Contains("PCIEX16_1", console.Output);
        Assert.Contains("PCI Express x16 Slot", console.Output);
        Assert.Contains("Usage: In Use", console.Output);
        Assert.Contains("Width: 64-bit", console.Output);
        Assert.Contains("Manufacturer: ASUSTeK", console.Output);
        Assert.Contains("PME Signal: True", console.Output);
        Assert.Contains("Hot Plug: False", console.Output);
        Assert.Contains("Status: OK", console.Output);
    }

    [Fact]
    public async Task DumpSystemSlots_When_Empty_Writes_Header_Only()
    {
        using var console = new TestConsoleWriter();

        await CliDemos.DumpSystemSlotsAsync(DemoProviders.Empty("Win32_SystemSlot"), console.Writer, TestContext.Current.CancellationToken);

        Assert.Contains("System Slots", console.Output);
        Assert.DoesNotContain("PCIEX16_1", console.Output);
    }
}
