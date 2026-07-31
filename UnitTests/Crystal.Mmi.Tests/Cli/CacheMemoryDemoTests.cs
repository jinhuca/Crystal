using Crystal.Mmi.Cli;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.Cli;

public sealed class CacheMemoryDemoTests
{
    [Fact]
    public async Task DumpCacheMemory_Writes_Header()
    {
        using var console = new TestConsoleWriter();

        await CliDemos.DumpCacheMemoryAsync(DemoProviders.CacheMemory(), console.Writer, TestContext.Current.CancellationToken);

        Assert.Contains("Cache Memory", console.Output);
    }

    [Fact]
    public async Task DumpCacheMemory_Writes_Core_Values()
    {
        using var console = new TestConsoleWriter();

        await CliDemos.DumpCacheMemoryAsync(DemoProviders.CacheMemory(), console.Writer, TestContext.Current.CancellationToken);

        Assert.Contains("L3 Cache", console.Output);
        Assert.Contains("Level: L3", console.Output);
        Assert.Contains("Type: Unified", console.Output);
        Assert.Contains("Associativity: 8-way Set-Associative", console.Output);
        Assert.Contains("Status: OK", console.Output);
    }

    [Fact]
    public async Task DumpCacheMemory_When_Empty_Writes_Header_Only()
    {
        using var console = new TestConsoleWriter();

        await CliDemos.DumpCacheMemoryAsync(DemoProviders.Empty("Win32_CacheMemory"), console.Writer, TestContext.Current.CancellationToken);

        Assert.Contains("Cache Memory", console.Output);
        Assert.DoesNotContain("L3 Cache", console.Output);
    }
}
