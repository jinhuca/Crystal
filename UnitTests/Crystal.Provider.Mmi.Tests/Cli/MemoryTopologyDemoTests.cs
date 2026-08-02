using Crystal.Provider.Mmi.Cli;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.Cli;

public sealed class MemoryTopologyDemoTests
{
    [Fact]
    public async Task DumpMemoryTopology_Writes_Summary()
    {
        using var console = new TestConsoleWriter();

        await CliDemos.DumpMemoryTopologyAsync(DemoProviders.MemoryTopology(), console.Writer, TestContext.Current.CancellationToken);

        Assert.Contains("Memory Topology", console.Output);
        Assert.Contains("Arrays Found: 1", console.Output);
        Assert.Contains("DIMM Count: 2", console.Output);
        Assert.Contains("Installed RAM: 32.0 GB", console.Output);
        Assert.Contains("L3", console.Output);
    }
}
