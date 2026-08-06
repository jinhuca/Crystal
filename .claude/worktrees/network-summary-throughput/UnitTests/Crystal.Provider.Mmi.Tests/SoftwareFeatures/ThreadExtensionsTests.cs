using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.SoftwareFeatures.Thread;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.SoftwareFeatures;

public class ThreadExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> ThreadRow(
        string handle = "5000", int threadState = 2) => WmiRow.Build(
        ("Caption", new WmiValue("Win32 Thread")),
        ("CreationClassName", new WmiValue("Win32_Thread")),
        ("Description", new WmiValue("Win32 Thread")),
        ("Handle", new WmiValue(handle)),
        ("ProcessHandle", new WmiValue("1234")),
        ("ProcessCreationClassName", new WmiValue("Win32_Process")),
        ("ThreadState", new WmiValue(threadState)),
        ("ThreadWaitReason", new WmiValue(5)),
        ("Priority", new WmiValue(8)),
        ("ExecutionState", new WmiValue(2)),
        ("StartAddress", new WmiValue(123_456_789)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("ElapsedTime", new WmiValue(60_000_000UL)),
        ("KernelModeTime", new WmiValue(5_000_000UL)),
        ("UserModeTime", new WmiValue(15_000_000UL)),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("InstallationDate", new WmiValue(new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc)))
    );

    [Fact]
    public async Task FullData_Maps_Handle()
    {
        var provider = new FakeWmiProvider("Win32_Thread", new[] { ThreadRow() });
        var results = await provider.ToSafeThreadMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("5000", results[0].Handle);
    }

    [Fact]
    public async Task FullData_Maps_ProcessHandle()
    {
        var provider = new FakeWmiProvider("Win32_Thread", new[] { ThreadRow() });
        var results = await provider.ToSafeThreadMetricsAsync(CancellationToken.None);

        Assert.Equal("1234", results[0].ProcessHandle);
    }

    [Fact]
    public async Task FullData_Maps_ThreadState_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Thread", new[] { ThreadRow(threadState: 2) });
        var results = await provider.ToSafeThreadMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)2, results[0].ThreadState);
    }

    [Fact]
    public async Task FullData_Maps_Priority_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Thread", new[] { ThreadRow() });
        var results = await provider.ToSafeThreadMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)8, results[0].Priority);
    }

    [Fact]
    public async Task FullData_Maps_ElapsedTime_ULong()
    {
        var provider = new FakeWmiProvider("Win32_Thread", new[] { ThreadRow() });
        var results = await provider.ToSafeThreadMetricsAsync(CancellationToken.None);

        Assert.Equal(60_000_000UL, results[0].ElapsedTime);
    }

    [Fact]
    public async Task FullData_Maps_KernelModeTime_ULong()
    {
        var provider = new FakeWmiProvider("Win32_Thread", new[] { ThreadRow() });
        var results = await provider.ToSafeThreadMetricsAsync(CancellationToken.None);

        Assert.Equal(5_000_000UL, results[0].KernelModeTime);
    }

    [Fact]
    public async Task FullData_Maps_UserModeTime_ULong()
    {
        var provider = new FakeWmiProvider("Win32_Thread", new[] { ThreadRow() });
        var results = await provider.ToSafeThreadMetricsAsync(CancellationToken.None);

        Assert.Equal(15_000_000UL, results[0].UserModeTime);
    }

    [Fact]
    public async Task FullData_Maps_StartAddress_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Thread", new[] { ThreadRow() });
        var results = await provider.ToSafeThreadMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)123_456_789, results[0].StartAddress);
    }

    [Fact]
    public async Task FullData_Maps_StatusInfo_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_Thread", new[] { ThreadRow() });
        var results = await provider.ToSafeThreadMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].StatusInfo);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate_DateTime()
    {
        var provider = new FakeWmiProvider("Win32_Thread", new[] { ThreadRow() });
        var results = await provider.ToSafeThreadMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task MultipleThreads_Returns_All()
    {
        var t1 = ThreadRow("1001", 2);
        var t2 = ThreadRow("1002", 5);
        var t3 = ThreadRow("1003", 0);

        var provider = new FakeWmiProvider("Win32_Thread", new[] { t1, t2, t3 });
        var results = await provider.ToSafeThreadMetricsAsync(CancellationToken.None);

        Assert.Equal(3, results.Count);
        Assert.Equal("1001", results[0].Handle);
        Assert.Equal("1002", results[1].Handle);
        Assert.Equal("1003", results[2].Handle);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_Thread", WmiRow.Empty());
        var results = await provider.ToSafeThreadMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingKeys_Returns_Null_Fields()
    {
        var row = WmiRow.Build(("Handle", new WmiValue("9999")));
        var provider = new FakeWmiProvider("Win32_Thread", new[] { row });
        var results = await provider.ToSafeThreadMetricsAsync(CancellationToken.None);

        Assert.Equal("9999", results[0].Handle);
        Assert.Null(results[0].ThreadState);
        Assert.Null(results[0].Priority);
        Assert.Null(results[0].ElapsedTime);
    }

    // ── ThreadStatePhrase ─────────────────────────────────────────────────

    [Theory]
    [InlineData(0u, "Initialized")]
    [InlineData(1u, "Ready")]
    [InlineData(2u, "Running (Active)")]
    [InlineData(3u, "Transition")]
    [InlineData(4u, "Terminated")]
    [InlineData(5u, "Waiting / Blocked")]
    [InlineData(6u, "Transition Space")]
    [InlineData(99u, "Unknown State")]
    public void ThreadStatePhrase_Maps_All_Known_States(uint state, string expected)
    {
        var m = MakeThreadWithState(state, null);
        Assert.Equal(expected, m.ThreadStatePhrase);
    }

    [Fact]
    public void ThreadStatePhrase_Null_ThreadState_Returns_Unknown()
    {
        var m = MakeThreadWithState(null, null);
        Assert.Equal("Unknown State", m.ThreadStatePhrase);
    }

    // ── WaitReasonPhrase ───────────────────────────────────────────────────

    [Theory]
    [InlineData(0u, "Executive / Core Lock")]
    [InlineData(7u, "Executive / Core Lock")]
    [InlineData(1u, "Free Page Allocation")]
    [InlineData(8u, "Free Page Allocation")]
    [InlineData(2u, "Page In Transit")]
    [InlineData(9u, "Page In Transit")]
    [InlineData(3u, "Pool Allocation Block")]
    [InlineData(10u, "Pool Allocation Block")]
    [InlineData(4u, "Delay Execution Timer")]
    [InlineData(11u, "Delay Execution Timer")]
    [InlineData(5u, "Suspended / Frozen State")]
    [InlineData(12u, "Suspended / Frozen State")]
    [InlineData(6u, "User Request Block")]
    [InlineData(13u, "User Request Block")]
    [InlineData(18u, "Event Pair Delay")]
    [InlineData(19u, "LPC Receive Delay")]
    [InlineData(20u, "LPC Reply Delay")]
    [InlineData(99u, "Not Waiting (Running)")]
    public void WaitReasonPhrase_Maps_All_Known_Reasons(uint reason, string expected)
    {
        var m = MakeThreadWithState(2u, reason);
        Assert.Equal(expected, m.WaitReasonPhrase);
    }

    [Fact]
    public void WaitReasonPhrase_Null_Returns_Not_Waiting()
    {
        var m = MakeThreadWithState(2u, null);
        Assert.Equal("Not Waiting (Running)", m.WaitReasonPhrase);
    }

    private static ThreadMetrics MakeThreadWithState(uint? threadState, uint? waitReason)
        => new ThreadMetrics(
            null, null, null, null, null, "T1", null, null,
            null, null, null, null, null, null, null, null, null,
            threadState, waitReason, null);
}
