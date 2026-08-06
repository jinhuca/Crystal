using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.SoftwareFeatures.Process;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.SoftwareFeatures;

public class ProcessExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> ChromeRow() => WmiRow.Build(
        ("Caption", new WmiValue("chrome.exe")),
        ("Name", new WmiValue("chrome.exe")),
        ("CommandLine", new WmiValue("\"C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe\"")),
        ("ExecutablePath", new WmiValue("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe")),
        ("Description", new WmiValue("Google Chrome")),
        ("Handle", new WmiValue(1234)),
        ("ProcessId", new WmiValue(1234)),
        ("ParentProcessId", new WmiValue(4)),
        ("HandleCount", new WmiValue(500)),
        ("PageFaults", new WmiValue(1024)),
        ("PageFileUsage", new WmiValue(2048)),
        ("PeakPageFileUsage", new WmiValue(4096)),
        ("Priority", new WmiValue(8)),
        ("SessionId", new WmiValue(1)),
        ("Status", new WmiValue("OK")),
        ("WindowsVersion", new WmiValue("10.0.22631")),
        ("ExecutionState", new WmiValue(0)),
        ("MaximumWorkingSetSize", new WmiValue(1413120)),
        ("MinimumWorkingSetSize", new WmiValue(204800)),
        ("WorkingSetSize", new WmiValue(104_857_600UL)),   // 100 MB
        ("VirtualSize", new WmiValue(2_147_483_648UL)),    // 2 GB
        ("PeakVirtualSize", new WmiValue(2_500_000_000UL)),
        ("PeakWorkingSetSize", new WmiValue(200_000_000UL)),
        ("PrivatePageCount", new WmiValue(90_000_000UL)),
        ("KernelModeTime", new WmiValue(10_000_000UL)),
        ("UserModeTime", new WmiValue(50_000_000UL)),
        ("ReadOperationCount", new WmiValue(1_000UL)),
        ("WriteOperationCount", new WmiValue(500UL)),
        ("OtherOperationCount", new WmiValue(200UL)),
        ("ReadTransferCount", new WmiValue(1_048_576UL)),
        ("WriteTransferCount", new WmiValue(524_288UL)),
        ("OtherTransferCount", new WmiValue(4096UL)),
        ("CreationDate", new WmiValue(new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc))),
        ("CreationClassName", new WmiValue("Win32_Process"))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_Process", new[] { ChromeRow() });
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("chrome.exe", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_CommandLine()
    {
        var provider = new FakeWmiProvider("Win32_Process", new[] { ChromeRow() });
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.Contains("chrome.exe", results[0].CommandLine);
    }

    [Fact]
    public async Task FullData_Maps_ProcessId_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Process", new[] { ChromeRow() });
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)1234, results[0].ProcessId);
    }

    [Fact]
    public async Task FullData_Maps_Handle_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Process", new[] { ChromeRow() });
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)1234, results[0].Handle);
    }

    [Fact]
    public async Task FullData_Maps_ParentProcessId_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Process", new[] { ChromeRow() });
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)4, results[0].ParentProcessId);
    }

    [Fact]
    public async Task FullData_Maps_WorkingSetSize_ULong()
    {
        var provider = new FakeWmiProvider("Win32_Process", new[] { ChromeRow() });
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.Equal(104_857_600UL, results[0].WorkingSetSize);
    }

    [Fact]
    public async Task FullData_Maps_VirtualSize_ULong()
    {
        var provider = new FakeWmiProvider("Win32_Process", new[] { ChromeRow() });
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.Equal(2_147_483_648UL, results[0].VirtualSize);
    }

    [Fact]
    public async Task FullData_Maps_KernelModeTime_ULong()
    {
        var provider = new FakeWmiProvider("Win32_Process", new[] { ChromeRow() });
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.Equal(10_000_000UL, results[0].KernelModeTime);
    }

    [Fact]
    public async Task FullData_Maps_UserModeTime_ULong()
    {
        var provider = new FakeWmiProvider("Win32_Process", new[] { ChromeRow() });
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.Equal(50_000_000UL, results[0].UserModeTime);
    }

    [Fact]
    public async Task FullData_Maps_CreationDate_DateTime()
    {
        var provider = new FakeWmiProvider("Win32_Process", new[] { ChromeRow() });
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc), results[0].CreationDate);
    }

    [Fact]
    public async Task FullData_Maps_Priority_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Process", new[] { ChromeRow() });
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)8, results[0].Priority);
    }

    [Fact]
    public async Task FullData_Maps_SessionId_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Process", new[] { ChromeRow() });
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)1, results[0].SessionId);
    }

    [Fact]
    public async Task MultipleProcesses_Returns_All()
    {
        var p1 = WmiRow.Build(("Name", new WmiValue("notepad.exe")), ("ProcessId", new WmiValue(1000)));
        var p2 = WmiRow.Build(("Name", new WmiValue("explorer.exe")), ("ProcessId", new WmiValue(2000)));

        var provider = new FakeWmiProvider("Win32_Process", new[] { p1, p2 });
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("notepad.exe", results[0].Name);
        Assert.Equal("explorer.exe", results[1].Name);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_Process", WmiRow.Empty());
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    // ── WorkingSetInMB ─────────────────────────────────────────────────────

    [Fact]
    public async Task WorkingSetInMB_Computed_Correctly()
    {
        // WorkingSetSize = 104_857_600 bytes = 100.0 MB
        var provider = new FakeWmiProvider("Win32_Process", new[] { ChromeRow() });
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.NotNull(results[0].WorkingSetInMB);
        Assert.Equal(100.0, results[0].WorkingSetInMB!.Value, precision: 2);
    }

    [Fact]
    public async Task WorkingSetInMB_Null_When_Missing()
    {
        var row = WmiRow.Build(("Name", new WmiValue("proc.exe")));
        var provider = new FakeWmiProvider("Win32_Process", new[] { row });
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.Null(results[0].WorkingSetInMB);
    }

    // ── VirtualSizeInMB ────────────────────────────────────────────────────

    [Fact]
    public async Task VirtualSizeInMB_Computed_Correctly()
    {
        // VirtualSize = 2_147_483_648 bytes = 2048 MB
        var provider = new FakeWmiProvider("Win32_Process", new[] { ChromeRow() });
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.NotNull(results[0].VirtualSizeInMB);
        Assert.Equal(2048.0, results[0].VirtualSizeInMB!.Value, precision: 2);
    }

    [Fact]
    public async Task VirtualSizeInMB_Null_When_Missing()
    {
        var row = WmiRow.Build(("Name", new WmiValue("proc.exe")));
        var provider = new FakeWmiProvider("Win32_Process", new[] { row });
        var results = await provider.ToSafeProcessMetricsAsync(CancellationToken.None);

        Assert.Null(results[0].VirtualSizeInMB);
    }
}
