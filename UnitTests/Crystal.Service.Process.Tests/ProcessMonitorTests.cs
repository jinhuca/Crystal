using Crystal.Provider.Etw;
using Crystal.Provider.Mmi.MmiEngine;
using Microsoft.Reactive.Testing;
using System.Collections.Frozen;
using Xunit;

namespace Crystal.Service.Process.Tests;

public class ProcessMonitorTests {
  private const ulong MB = 1024UL * 1024;
  private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

  private static ProcessMonitor Monitor(
      IReadOnlyList<FrozenDictionary<string, WmiValue>> rows,
      TestScheduler scheduler,
      FakeEtwSource? etw = null) {
    var broadcaster = etw is null ? null : new EtwRateBroadcaster(etw, Interval, scheduler);
    return new ProcessMonitor(new FakeWmiHardwareProvider(rows), broadcaster, Interval, scheduler);
  }

  // The first sample fires synchronously on subscription; each scheduler interval yields another.
  // Returns every sample emitted through the given number of advanced intervals.
  private static List<IReadOnlyList<ProcessSample>> Poll(ProcessMonitor monitor, TestScheduler scheduler, int intervals) {
    var samples = new List<IReadOnlyList<ProcessSample>>();
    using var _ = monitor.Samples.Subscribe(samples.Add);
    for (var i = 0; i < intervals; i++) scheduler.AdvanceBy(Interval.Ticks);
    return samples;
  }

  [Fact]
  public void FirstSample_MapsNameWorkingSetAndStatus() {
    var scheduler = new TestScheduler();
    var monitor = Monitor(
        [ProcessRows.Row(processId: 100, name: "chrome.exe", workingSet: 512 * MB,
                         sessionId: 1, status: "OK", executablePath: @"C:\chrome.exe")],
        scheduler);

    var first = Poll(monitor, scheduler, intervals: 0)[0];

    var sample = Assert.Single(first);
    Assert.Equal(100u, sample.ProcessId);
    Assert.Equal("chrome.exe", sample.Name);
    Assert.Equal(512.0, sample.WorkingSetMb);
    Assert.Equal("OK", sample.Status);
    Assert.Equal(@"C:\chrome.exe", sample.ExecutablePath);
  }

  [Fact]
  public void Session0Process_IsCategorizedAsWindowsProcess() {
    var scheduler = new TestScheduler();
    var monitor = Monitor(
        [ProcessRows.Row(processId: 4, name: "services.exe", workingSet: 8 * MB, sessionId: 0)],
        scheduler);

    var sample = Assert.Single(Poll(monitor, scheduler, intervals: 0)[0]);

    Assert.Equal(ProcessCategory.WindowsProcess, sample.Category);
  }

  [Fact]
  public void InteractiveProcessWithoutWindow_IsBackgroundProcess() {
    var scheduler = new TestScheduler();
    // A synthetic PID in an interactive session that owns no visible window → Background Process.
    var monitor = Monitor(
        [ProcessRows.Row(processId: 987654, name: "helper.exe", workingSet: 4 * MB, sessionId: 1)],
        scheduler);

    var sample = Assert.Single(Poll(monitor, scheduler, intervals: 0)[0]);

    Assert.Equal(ProcessCategory.BackgroundProcess, sample.Category);
  }

  [Fact]
  public void MissingName_FallsBackToUnknownAndBlankStatusBecomesRunning() {
    var scheduler = new TestScheduler();
    var monitor = Monitor([ProcessRows.Row(processId: 50, workingSet: MB, sessionId: 1)], scheduler);

    var sample = Assert.Single(Poll(monitor, scheduler, intervals: 0)[0]);

    Assert.Equal("(unknown)", sample.Name);
    Assert.Equal("Running", sample.Status);
    Assert.Null(sample.ExecutablePath);   // empty path → null so the UI can show a generic icon
  }

  [Fact]
  public void RowsWithoutProcessId_AreSkipped() {
    var scheduler = new TestScheduler();
    var monitor = Monitor(
        [ProcessRows.Row(name: "no-pid.exe", workingSet: MB, sessionId: 1),
         ProcessRows.Row(processId: 200, name: "real.exe", workingSet: MB, sessionId: 1)],
        scheduler);

    var sample = Assert.Single(Poll(monitor, scheduler, intervals: 0)[0]);

    Assert.Equal(200u, sample.ProcessId);
  }

  [Fact]
  public void FirstSample_HasNoEtwOverlay_BecauseBroadcasterHasNotEmittedYet() {
    var scheduler = new TestScheduler();
    var etw = new FakeEtwSource(isRunning: true, startError: null,
        new Dictionary<uint, ProcessEtwMetrics> { [300] = new(DiskBytesPerSec: 1, NetBytesPerSec: 2, GpuPercent: 3) });
    var monitor = Monitor([ProcessRows.Row(processId: 300, name: "p.exe", workingSet: MB, sessionId: 1)],
        scheduler, etw);

    var sample = Assert.Single(Poll(monitor, scheduler, intervals: 0)[0]);

    // Before the first broadcast the overlay is unknown, not a real zero → null placeholders.
    Assert.Null(sample.GpuPercent);
    Assert.Null(sample.DiskBytesPerSec);
    Assert.Null(sample.NetBytesPerSec);
  }

  [Fact]
  public void LaterSample_OverlaysEtwRatesByPid() {
    var scheduler = new TestScheduler();
    var etw = new FakeEtwSource(isRunning: true, startError: null,
        new Dictionary<uint, ProcessEtwMetrics> {
          [300] = new(DiskBytesPerSec: 4096, NetBytesPerSec: 8192, GpuPercent: 25),
        });
    var monitor = Monitor([ProcessRows.Row(processId: 300, name: "p.exe", workingSet: MB, sessionId: 1)],
        scheduler, etw);

    // Advance past the broadcaster's first tick so a poll sees the published rates.
    var samples = Poll(monitor, scheduler, intervals: 2);
    var last = samples[^1];

    var sample = Assert.Single(last);
    Assert.Equal(25, sample.GpuPercent);
    Assert.Equal(4096, sample.DiskBytesPerSec);
    Assert.Equal(8192, sample.NetBytesPerSec);
  }

  [Fact]
  public void LaterSample_PidWithNoEtwActivity_ReadsRealZeroOnceEtwIsRunning() {
    var scheduler = new TestScheduler();
    // ETW is running but reports nothing for PID 300 → a real zero, not the "unwired" placeholder.
    var etw = new FakeEtwSource(isRunning: true, startError: null,
        new Dictionary<uint, ProcessEtwMetrics> {
          [999] = new(DiskBytesPerSec: 1, NetBytesPerSec: 1, GpuPercent: 1),
        });
    var monitor = Monitor([ProcessRows.Row(processId: 300, name: "p.exe", workingSet: MB, sessionId: 1)],
        scheduler, etw);

    var last = Poll(monitor, scheduler, intervals: 2)[^1];

    var sample = Assert.Single(last);
    Assert.Equal(0, sample.GpuPercent);
    Assert.Equal(0, sample.DiskBytesPerSec);
    Assert.Equal(0, sample.NetBytesPerSec);
  }

  [Fact]
  public void MetricsStatusError_SurfacesWhenEtwIsNotRunning() {
    var scheduler = new TestScheduler();
    var etw = new FakeEtwSource(isRunning: false, startError: "not elevated",
        new Dictionary<uint, ProcessEtwMetrics>());
    var monitor = Monitor([ProcessRows.Row(processId: 1, name: "p.exe", workingSet: MB, sessionId: 1)],
        scheduler, etw);

    Assert.Equal("not elevated", monitor.MetricsStatusError);
  }

  [Fact]
  public void MetricsStatusError_NullWhenNoEtwSource() {
    var scheduler = new TestScheduler();
    var monitor = Monitor([ProcessRows.Row(processId: 1, name: "p.exe", workingSet: MB, sessionId: 1)], scheduler);

    Assert.Null(monitor.MetricsStatusError);
  }

  [Fact]
  public void NoSubscribers_DoesNotPoll() {
    var scheduler = new TestScheduler();
    var monitor = Monitor([ProcessRows.Row(processId: 1, name: "p.exe", workingSet: MB, sessionId: 1)], scheduler);

    scheduler.AdvanceBy(Interval.Ticks * 5);

    // Ref-counted: with nobody subscribed the poll timer never ran. Subscribing now still yields the
    // synchronous first sample, proving the stream is cold rather than already-drained.
    var samples = Poll(monitor, scheduler, intervals: 0);
    Assert.Single(samples[0]);
  }
}
