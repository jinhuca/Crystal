using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.PerformanceFeatures.PerfCounter;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.PerformanceFeatures;

public class PerfCounterExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> FullPerfCounterRow() => WmiRow.Build(
        ("Caption",            new WmiValue("Win32_Perf counter")),
        ("Description",        new WmiValue("Base performance counter object")),
        ("Name",               new WmiValue("_Total")),
        ("Frequency_Object",   new WmiValue((ulong)10_000_000)),
        ("Frequency_PerfTime", new WmiValue((ulong)3_579_545)),
        ("Frequency_Sys100NS", new WmiValue((ulong)10_000_000)),
        ("Timestamp_Object",   new WmiValue((ulong)133_500_000_000_000UL)),
        ("Timestamp_PerfTime", new WmiValue((ulong)123_456_789UL)),
        ("Timestamp_Sys100NS", new WmiValue((ulong)133_500_000_000_000UL))
    );

    // --- Field mapping ---

    [Fact]
    public async Task FullData_Maps_Caption()
    {
        var provider = new FakeWmiProvider("Win32_Perf", new[] { FullPerfCounterRow() });
        var results = await provider.ToSafePerfCounterMetricsAsync(CancellationToken.None);
        Assert.Equal("Win32_Perf counter", results[0].Caption);
    }

    [Fact]
    public async Task FullData_Maps_Description()
    {
        var provider = new FakeWmiProvider("Win32_Perf", new[] { FullPerfCounterRow() });
        var results = await provider.ToSafePerfCounterMetricsAsync(CancellationToken.None);
        Assert.Equal("Base performance counter object", results[0].Description);
    }

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_Perf", new[] { FullPerfCounterRow() });
        var results = await provider.ToSafePerfCounterMetricsAsync(CancellationToken.None);
        Assert.Equal("_Total", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_Frequency_Object()
    {
        var provider = new FakeWmiProvider("Win32_Perf", new[] { FullPerfCounterRow() });
        var results = await provider.ToSafePerfCounterMetricsAsync(CancellationToken.None);
        Assert.Equal((ulong)10_000_000, results[0].Frequency_Object);
    }

    [Fact]
    public async Task FullData_Maps_Frequency_PerfTime()
    {
        var provider = new FakeWmiProvider("Win32_Perf", new[] { FullPerfCounterRow() });
        var results = await provider.ToSafePerfCounterMetricsAsync(CancellationToken.None);
        Assert.Equal((ulong)3_579_545, results[0].Frequency_PerfTime);
    }

    [Fact]
    public async Task FullData_Maps_Frequency_Sys100NS()
    {
        var provider = new FakeWmiProvider("Win32_Perf", new[] { FullPerfCounterRow() });
        var results = await provider.ToSafePerfCounterMetricsAsync(CancellationToken.None);
        Assert.Equal((ulong)10_000_000, results[0].Frequency_Sys100NS);
    }

    [Fact]
    public async Task FullData_Maps_Timestamp_Object()
    {
        var provider = new FakeWmiProvider("Win32_Perf", new[] { FullPerfCounterRow() });
        var results = await provider.ToSafePerfCounterMetricsAsync(CancellationToken.None);
        Assert.Equal((ulong)133_500_000_000_000UL, results[0].Timestamp_Object);
    }

    [Fact]
    public async Task FullData_Maps_Timestamp_PerfTime()
    {
        var provider = new FakeWmiProvider("Win32_Perf", new[] { FullPerfCounterRow() });
        var results = await provider.ToSafePerfCounterMetricsAsync(CancellationToken.None);
        Assert.Equal((ulong)123_456_789UL, results[0].Timestamp_PerfTime);
    }

    [Fact]
    public async Task FullData_Maps_Timestamp_Sys100NS()
    {
        var provider = new FakeWmiProvider("Win32_Perf", new[] { FullPerfCounterRow() });
        var results = await provider.ToSafePerfCounterMetricsAsync(CancellationToken.None);
        Assert.Equal((ulong)133_500_000_000_000UL, results[0].Timestamp_Sys100NS);
    }

    // --- Multi-instance ---

    [Fact]
    public async Task Multiple_Instances_Returns_All()
    {
        var r1 = WmiRow.Build(("Name", new WmiValue("_Total")),   ("Frequency_Object", new WmiValue((ulong)10_000_000)));
        var r2 = WmiRow.Build(("Name", new WmiValue("Processor")), ("Frequency_Object", new WmiValue((ulong)10_000_000)));

        var provider = new FakeWmiProvider("Win32_Perf", new[] { r1, r2 });
        var results = await provider.ToSafePerfCounterMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("_Total",    results[0].Name);
        Assert.Equal("Processor", results[1].Name);
    }

    // --- Fallback behaviour ---

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_Perf", WmiRow.Empty());
        var results = await provider.ToSafePerfCounterMetricsAsync(CancellationToken.None);
        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingKeys_Return_Null_For_Those_Fields()
    {
        var provider = new FakeWmiProvider("Win32_Perf",
            new[] { WmiRow.Build(("Name", new WmiValue("_Total"))) });
        var results = await provider.ToSafePerfCounterMetricsAsync(CancellationToken.None);

        Assert.Equal("_Total", results[0].Name);
        Assert.Null(results[0].Caption);
        Assert.Null(results[0].Description);
        Assert.Null(results[0].Frequency_Object);
        Assert.Null(results[0].Frequency_PerfTime);
        Assert.Null(results[0].Frequency_Sys100NS);
        Assert.Null(results[0].Timestamp_Object);
        Assert.Null(results[0].Timestamp_PerfTime);
        Assert.Null(results[0].Timestamp_Sys100NS);
    }

    [Fact]
    public async Task Cancelled_Token_Returns_Empty_Fallback()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var provider = new FakeWmiProvider("Win32_Perf", new[] { FullPerfCounterRow() });
        var results = await provider.ToSafePerfCounterMetricsAsync(cts.Token);

        Assert.Empty(results);
    }

    [Fact]
    public async Task WrongValueType_For_ULong_Key_Returns_Null()
    {
        // Frequency_Object stored as String instead of ULong — GetULong returns null
        var provider = new FakeWmiProvider("Win32_Perf",
            new[] { WmiRow.Build(("Frequency_Object", new WmiValue("10000000"))) });
        var results = await provider.ToSafePerfCounterMetricsAsync(CancellationToken.None);

        Assert.Null(results[0].Frequency_Object);
    }

    [Fact]
    public async Task WrongValueType_For_String_Key_Returns_Null()
    {
        // Name stored as Int instead of String — GetStr returns null
        var provider = new FakeWmiProvider("Win32_Perf",
            new[] { WmiRow.Build(("Name", new WmiValue(42))) });
        var results = await provider.ToSafePerfCounterMetricsAsync(CancellationToken.None);

        Assert.Null(results[0].Name);
    }

    // --- Record equality ---

    [Fact]
    public async Task Two_Identical_Records_Are_Equal()
    {
        var provider1 = new FakeWmiProvider("Win32_Perf", new[] { FullPerfCounterRow() });
        var provider2 = new FakeWmiProvider("Win32_Perf", new[] { FullPerfCounterRow() });

        var r1 = (await provider1.ToSafePerfCounterMetricsAsync(CancellationToken.None))[0];
        var r2 = (await provider2.ToSafePerfCounterMetricsAsync(CancellationToken.None))[0];

        Assert.Equal(r1, r2);
    }
}
