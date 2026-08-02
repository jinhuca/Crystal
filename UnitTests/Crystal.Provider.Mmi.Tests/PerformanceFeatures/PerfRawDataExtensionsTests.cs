using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.PerformanceFeatures.PerfRawData;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.PerformanceFeatures;

public class PerfRawDataExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> FullPerfRawDataRow() => WmiRow.Build(
        ("Caption",            new WmiValue("Win32_PerfRawData counter")),
        ("Description",        new WmiValue("Base raw performance data object")),
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
        var provider = new FakeWmiProvider("Win32_PerfRawData", new[] { FullPerfRawDataRow() });
        var results = await provider.ToSafePerfRawDataMetricsAsync(CancellationToken.None);
        Assert.Equal("Win32_PerfRawData counter", results[0].Caption);
    }

    [Fact]
    public async Task FullData_Maps_Description()
    {
        var provider = new FakeWmiProvider("Win32_PerfRawData", new[] { FullPerfRawDataRow() });
        var results = await provider.ToSafePerfRawDataMetricsAsync(CancellationToken.None);
        Assert.Equal("Base raw performance data object", results[0].Description);
    }

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_PerfRawData", new[] { FullPerfRawDataRow() });
        var results = await provider.ToSafePerfRawDataMetricsAsync(CancellationToken.None);
        Assert.Equal("0,1,2,3", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_Frequency_Object()
    {
        var provider = new FakeWmiProvider("Win32_PerfRawData", new[] { FullPerfRawDataRow() });
        var results = await provider.ToSafePerfRawDataMetricsAsync(CancellationToken.None);
        Assert.Equal((ulong)10_000_000, results[0].Frequency_Object);
    }

    [Fact]
    public async Task FullData_Maps_Frequency_PerfTime()
    {
        var provider = new FakeWmiProvider("Win32_PerfRawData", new[] { FullPerfRawDataRow() });
        var results = await provider.ToSafePerfRawDataMetricsAsync(CancellationToken.None);
        Assert.Equal((ulong)3_579_545, results[0].Frequency_PerfTime);
    }

    [Fact]
    public async Task FullData_Maps_Frequency_Sys100NS()
    {
        var provider = new FakeWmiProvider("Win32_PerfRawData", new[] { FullPerfRawDataRow() });
        var results = await provider.ToSafePerfRawDataMetricsAsync(CancellationToken.None);
        Assert.Equal((ulong)10_000_000, results[0].Frequency_Sys100NS);
    }

    [Fact]
    public async Task FullData_Maps_Timestamp_Object()
    {
        var provider = new FakeWmiProvider("Win32_PerfRawData", new[] { FullPerfRawDataRow() });
        var results = await provider.ToSafePerfRawDataMetricsAsync(CancellationToken.None);
        Assert.Equal((ulong)133_600_000_000_000UL, results[0].Timestamp_Object);
    }

    [Fact]
    public async Task FullData_Maps_Timestamp_PerfTime()
    {
        var provider = new FakeWmiProvider("Win32_PerfRawData", new[] { FullPerfRawDataRow() });
        var results = await provider.ToSafePerfRawDataMetricsAsync(CancellationToken.None);
        Assert.Equal((ulong)987_654_321UL, results[0].Timestamp_PerfTime);
    }

    [Fact]
    public async Task FullData_Maps_Timestamp_Sys100NS()
    {
        var provider = new FakeWmiProvider("Win32_PerfRawData", new[] { FullPerfRawDataRow() });
        var results = await provider.ToSafePerfRawDataMetricsAsync(CancellationToken.None);
        Assert.Equal((ulong)133_600_000_000_000UL, results[0].Timestamp_Sys100NS);
    }

    // --- Multi-instance ---

    [Fact]
    public async Task Multiple_Instances_Returns_All()
    {
        var r1 = WmiRow.Build(("Name", new WmiValue("0")), ("Frequency_PerfTime", new WmiValue((ulong)3_579_545)));
        var r2 = WmiRow.Build(("Name", new WmiValue("1")), ("Frequency_PerfTime", new WmiValue((ulong)3_579_545)));
        var r3 = WmiRow.Build(("Name", new WmiValue("2")), ("Frequency_PerfTime", new WmiValue((ulong)3_579_545)));

        var provider = new FakeWmiProvider("Win32_PerfRawData", new[] { r1, r2, r3 });
        var results = await provider.ToSafePerfRawDataMetricsAsync(CancellationToken.None);

        Assert.Equal(3, results.Count);
        Assert.Equal("0", results[0].Name);
        Assert.Equal("1", results[1].Name);
        Assert.Equal("2", results[2].Name);
    }

    // --- Fallback behaviour ---

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_PerfRawData", WmiRow.Empty());
        var results = await provider.ToSafePerfRawDataMetricsAsync(CancellationToken.None);
        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingKeys_Return_Null_For_Those_Fields()
    {
        var provider = new FakeWmiProvider("Win32_PerfRawData",
            new[] { WmiRow.Build(("Name", new WmiValue("0"))) });
        var results = await provider.ToSafePerfRawDataMetricsAsync(CancellationToken.None);

        Assert.Equal("0", results[0].Name);
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

        var provider = new FakeWmiProvider("Win32_PerfRawData", new[] { FullPerfRawDataRow() });
        var results = await provider.ToSafePerfRawDataMetricsAsync(cts.Token);

        Assert.Empty(results);
    }

    [Fact]
    public async Task WrongValueType_For_ULong_Key_Returns_Null()
    {
        // Timestamp_Object stored as String instead of ULong — GetULong returns null
        var provider = new FakeWmiProvider("Win32_PerfRawData",
            new[] { WmiRow.Build(("Timestamp_Object", new WmiValue("133600000000000"))) });
        var results = await provider.ToSafePerfRawDataMetricsAsync(CancellationToken.None);

        Assert.Null(results[0].Timestamp_Object);
    }

    [Fact]
    public async Task WrongValueType_For_String_Key_Returns_Null()
    {
        // Caption stored as Bool instead of String — GetStr returns null
        var provider = new FakeWmiProvider("Win32_PerfRawData",
            new[] { WmiRow.Build(("Caption", new WmiValue(true))) });
        var results = await provider.ToSafePerfRawDataMetricsAsync(CancellationToken.None);

        Assert.Null(results[0].Caption);
    }

    // --- Record equality ---

    [Fact]
    public async Task Two_Identical_Records_Are_Equal()
    {
        var provider1 = new FakeWmiProvider("Win32_PerfRawData", new[] { FullPerfRawDataRow() });
        var provider2 = new FakeWmiProvider("Win32_PerfRawData", new[] { FullPerfRawDataRow() });

        var r1 = (await provider1.ToSafePerfRawDataMetricsAsync(CancellationToken.None))[0];
        var r2 = (await provider2.ToSafePerfRawDataMetricsAsync(CancellationToken.None))[0];

        Assert.Equal(r1, r2);
    }
}
