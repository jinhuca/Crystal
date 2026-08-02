using Crystal.Provider.Mmi.Cli;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.Cli;

public sealed class PhysicalMemoryArrayDemoTests
{
    [Fact]
    public async Task DumpPhysicalMemoryArrays_Writes_Header()
    {
        using var console = new TestConsoleWriter();

        await CliDemos.DumpPhysicalMemoryArraysAsync(DemoProviders.PhysicalMemoryArray(), console.Writer, TestContext.Current.CancellationToken);

        Assert.Contains("Physical Memory Arrays", console.Output);
    }

    [Fact]
    public async Task DumpPhysicalMemoryArrays_Writes_Core_Values()
    {
        using var console = new TestConsoleWriter();

        await CliDemos.DumpPhysicalMemoryArraysAsync(DemoProviders.PhysicalMemoryArray(), console.Writer, TestContext.Current.CancellationToken);

        Assert.Contains("Physical Memory Array", console.Output);
        Assert.Contains("Dell Inc.", console.Output);
        Assert.Contains("System Board or Motherboard", console.Output);
        Assert.Contains("System Memory", console.Output);
        Assert.Contains("Memory Devices: 4", console.Output);
        Assert.Contains("Multi-bit ECC", console.Output);
        Assert.Contains("Status: OK", console.Output);
    }

    [Fact]
    public async Task DumpPhysicalMemoryArrays_When_Empty_Writes_Header_Only()
    {
        using var console = new TestConsoleWriter();

        await CliDemos.DumpPhysicalMemoryArraysAsync(DemoProviders.Empty("Win32_PhysicalMemoryArray"), console.Writer, TestContext.Current.CancellationToken);

        Assert.Contains("Physical Memory Arrays", console.Output);
        Assert.DoesNotContain("Memory Devices:", console.Output);
    }
}
