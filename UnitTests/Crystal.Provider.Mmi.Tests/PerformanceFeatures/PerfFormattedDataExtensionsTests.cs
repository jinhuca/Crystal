using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.PerformanceFeatures.PerfFormattedData;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.PerformanceFeatures;

public class PerfFormattedDataExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> FullPerfFormattedDataRow() => WmiRow.Build(
        ("Caption",            new WmiValue("Win32_PerfFormattedData counter")),
        ("Description",        new WmiValue("Base calculated performance data object")),
        ("Name",               new WmiValue("0,1,2,3")),
        ("Frequency_Object",   new WmiValue((ulong)10_000_000)),
        ("Frequency_PerfTime", new WmiValue((ulong)3_579_545)),
        ("Frequency_Sys100NS", new WmiValue((ulong)10_000_000)),
        ("Timestamp_Object",   new WmiValue((ulong)133_600_000_000_000UL)),
        ("Timestamp_PerfTime", new WmiValue((ulong)987_654_321UL)),
        ("Timestamp_Sys100NS", new WmiValue((ulong)133_600_000_000_000UL))
    );

    // --- Field mapping ---

    [Fact]
    public async Task FullData_Maps_Caption()
    {
        var provider = new FakeWmiProvider("Win32_PerfFormattedData", new[] { FullPerfFormattedDataRow() });
        var results = await provider.ToSafePerfFormattedDataMetricsAsync(CancellationToken.None);
        Assert.Equal("Win32_PerfFormattedData counter", results[0].Caption);
    }

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_PerfFormattedData", new[] { FullPerfFormattedDataRow() });
        var results = await provider.ToSafePerfFormattedDataMetricsAsync(CancellationToken.None);
        Assert.Equal("0,1,2,3", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_Frequency_PerfTime()
    {
        var provider = new FakeWmiProvider("Win32_PerfFormattedData", new[] { FullPerfFormattedDataRow() });
        var results = await provider.ToSafePerfFormattedDataMetricsAsync(CancellationToken.None);
        Assert.Equal((ulong)3_579_545, results[0].Frequency_PerfTime);
    }

    [Fact]
    public async Task FullData_Maps_Timestamp_Sys100NS()
    {
        var provider = new FakeWmiProvider("Win32_PerfFormattedData", new[] { FullPerfFormattedDataRow() });
        var results = await provider.ToSafePerfFormattedDataMetricsAsync(CancellationToken.None);
        Assert.Equal((ulong)133_600_000_000_000UL, results[0].Timestamp_Sys100NS);
    }

    // --- Multi-instance ---

    [Fact]
    public async Task Multiple_Instances_Returns_All()
    {
        var r1 = WmiRow.Build(("Name", new WmiValue("0")));
        var r2 = WmiRow.Build(("Name", new WmiValue("1")));
        var r3 = WmiRow.Build(("Name", new WmiValue("2")));

        var provider = new FakeWmiProvider("Win32_PerfFormattedData", new[] { r1, r2, r3 });
        var results = await provider.ToSafePerfFormattedDataMetricsAsync(CancellationToken.None);

        Assert.Equal(3, results.Count);
        Assert.Equal("0", results[0].Name);
        Assert.Equal("1", results[1].Name);
        Assert.Equal("2", results[2].Name);
    }

    // --- Fallback behaviour ---

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_PerfFormattedData", WmiRow.Empty());
        var results = await provider.ToSafePerfFormattedDataMetricsAsync(CancellationToken.None);
        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafePerfFormattedDataMetricsAsync(CancellationToken.None);
        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingKeys_Return_Null_For_Those_Fields()
    {
        var provider = new FakeWmiProvider("Win32_PerfFormattedData",
            new[] { WmiRow.Build(("Name", new WmiValue("0"))) });
        var results = await provider.ToSafePerfFormattedDataMetricsAsync(CancellationToken.None);

        Assert.Equal("0", results[0].Name);
        Assert.Null(results[0].Caption);
        Assert.Null(results[0].Frequency_Object);
        Assert.Null(results[0].Timestamp_Object);
    }

    [Fact]
    public async Task Cancelled_Token_Returns_Empty_Fallback()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var provider = new FakeWmiProvider("Win32_PerfFormattedData", new[] { FullPerfFormattedDataRow() });
        var results = await provider.ToSafePerfFormattedDataMetricsAsync(cts.Token);

        Assert.Empty(results);
    }

    [Fact]
    public async Task WrongValueType_For_ULong_Key_Returns_Null()
    {
        // Timestamp_Object stored as String instead of ULong — GetULong returns null
        var provider = new FakeWmiProvider("Win32_PerfFormattedData",
            new[] { WmiRow.Build(("Timestamp_Object", new WmiValue("133600000000000"))) });
        var results = await provider.ToSafePerfFormattedDataMetricsAsync(CancellationToken.None);

        Assert.Null(results[0].Timestamp_Object);
    }

    [Fact]
    public async Task WrongValueType_For_String_Key_Returns_Null()
    {
        // Caption stored as Bool instead of String — GetStr returns null
        var provider = new FakeWmiProvider("Win32_PerfFormattedData",
            new[] { WmiRow.Build(("Caption", new WmiValue(true))) });
        var results = await provider.ToSafePerfFormattedDataMetricsAsync(CancellationToken.None);

        Assert.Null(results[0].Caption);
    }

    // --- Record equality ---

    [Fact]
    public async Task Two_Identical_Records_Are_Equal()
    {
        var provider1 = new FakeWmiProvider("Win32_PerfFormattedData", new[] { FullPerfFormattedDataRow() });
        var provider2 = new FakeWmiProvider("Win32_PerfFormattedData", new[] { FullPerfFormattedDataRow() });

        var r1 = (await provider1.ToSafePerfFormattedDataMetricsAsync(CancellationToken.None))[0];
        var r2 = (await provider2.ToSafePerfFormattedDataMetricsAsync(CancellationToken.None))[0];

        Assert.Equal(r1, r2);
    }
}
