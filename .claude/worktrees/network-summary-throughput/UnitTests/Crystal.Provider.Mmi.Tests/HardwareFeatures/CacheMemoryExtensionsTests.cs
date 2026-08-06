using Crystal.Provider.Mmi.HardwareFeatures.CacheMemory;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class CacheMemoryExtensionsTests
{
    [Fact]
    public async Task FullData_Maps_Core_And_Computed_Fields()
    {
        var row = WmiRow.Build(("Name", new WmiValue("L3 Cache")), ("DeviceID", new WmiValue("Cache Memory 0")), ("InstalledSize", new WmiValue(32768)), ("MaxCacheSize", new WmiValue(32768)), ("CacheType", new WmiValue(5)), ("Level", new WmiValue(5)), ("Associativity", new WmiValue(7)), ("Status", new WmiValue("OK")));
        var provider = new FakeWmiProvider("Win32_CacheMemory", new[] { row });
        var result = (await provider.ToSafeCacheMemoryMetricsAsync(CancellationToken.None))[0];
        Assert.Equal("L3 Cache", result.Name);
        Assert.Equal("Unified", result.CacheTypeName);
        Assert.Equal("L3", result.LevelName);
        Assert.Equal("8-way Set-Associative", result.AssociativityName);
    }
}
