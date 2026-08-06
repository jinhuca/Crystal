using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Crystal.Provider.Etw;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.SoftwareFeatures.Process;

namespace ProcessModule.Models;

/// <summary>
/// Polls the WMI process provider on a cadence and emits a per-poll list of <see cref="ProcessSample"/>.
/// CPU% is derived here because WMI's <c>Win32_Process</c> exposes only cumulative kernel/user
/// CPU time (100-ns units): the busy fraction is the rise in that total between two polls divided
/// by the wall-clock gap and the logical-core count, matching Task Manager's whole-machine scale.
/// <para>
/// Ref-counted: the poll timer only runs while something is subscribed. GPU/Disk/Network are left
/// null here; the ETW backend layers them on later.
/// </para>
/// </summary>
public sealed class ProcessMonitor {
  private readonly IWmiHardwareProvider _provider;
  private readonly IProcessEtwSource? _etw;
  private readonly int _logicalCores;
  private readonly IObservable<IReadOnlyList<ProcessSample>> _samples;

  // Cumulative CPU time (100-ns units) per PID from the previous poll, so we can diff.
  private readonly Dictionary<uint, ulong> _lastCpuTime = new();
  private long _lastTimestampTicks;

  public ProcessMonitor(IWmiHardwareProvider provider, IProcessEtwSource? etw = null,
                        TimeSpan? pollInterval = null, IScheduler? scheduler = null) {
    ArgumentNullException.ThrowIfNull(provider);
    _provider = provider;
    _etw = etw;
    _logicalCores = Environment.ProcessorCount;
    var interval = pollInterval ?? TimeSpan.FromSeconds(1);
    scheduler ??= DefaultScheduler.Instance;

    // Poll sequentially: run one sample to completion, wait the interval, then repeat. This is
    // deliberately NOT Interval().SelectMany(FromAsync) — Interval fires on a fixed cadence
    // regardless of whether the prior sample finished, so a poll that overran the interval would
    // let two SampleAsync calls run concurrently and corrupt the shared _lastCpuTime / _lastTimestamp
    // state (a Dictionary written from two threads throws IndexOutOfRangeException). One poll is ever
    // in flight here, so that mutable baseline needs no locking.
    _samples = Observable
        .Defer(() => Observable.FromAsync(SampleAsync))
        .Concat(Observable.Empty<IReadOnlyList<ProcessSample>>().Delay(interval, scheduler))
        .Repeat()
        .Publish()
        .RefCount();
  }

  public IObservable<IReadOnlyList<ProcessSample>> Samples => _samples;

  /// <summary>
  /// Null when the ETW backend is running (GPU/Disk/Network are live), otherwise a short reason it
  /// isn't — so the UI can explain blank columns instead of leaving the user guessing. Null too when
  /// no ETW source was supplied at all.
  /// </summary>
  public string? MetricsStatusError => _etw is { IsRunning: false } ? _etw.StartError : null;

  private async Task<IReadOnlyList<ProcessSample>> SampleAsync(CancellationToken ct) {
    var metrics = await _provider.ToSafeProcessMetricsAsync(ct);

    // ETW rates cover the window since the last snapshot; pull once per poll and overlay by PID.
    var etwRates = _etw?.SnapshotRates();

    // Snapshot which PIDs own a visible window this poll, to split Apps from Background Processes.
    var windowedPids = VisibleWindowScanner.GetPidsWithVisibleWindows();

    long nowTicks = Environment.TickCount64 * TimeSpan.TicksPerMillisecond;
    // Elapsed wall time since the previous poll, expressed in the same 100-ns units as CPU time.
    double elapsed100Ns = _lastTimestampTicks == 0 ? 0 : nowTicks - _lastTimestampTicks;
    _lastTimestampTicks = nowTicks;

    var seenCpuTime = new Dictionary<uint, ulong>(metrics.Count);
    var samples = new List<ProcessSample>(metrics.Count);

    foreach (var m in metrics) {
      if (m.ProcessId is not { } pid) continue;

      ulong cpuTime = (m.KernelModeTime ?? 0) + (m.UserModeTime ?? 0);
      seenCpuTime[pid] = cpuTime;

      double cpuPercent = 0;
      if (elapsed100Ns > 0 && _lastCpuTime.TryGetValue(pid, out var prev) && cpuTime >= prev) {
        // (busy 100-ns ticks / wall 100-ns ticks) spreads over every core, so divide by the count
        // to land on the whole-machine 0-100 scale Task Manager shows by default.
        cpuPercent = (cpuTime - prev) / elapsed100Ns / _logicalCores * 100.0;
        if (cpuPercent < 0) cpuPercent = 0;
        if (cpuPercent > 100) cpuPercent = 100;
      }

      double? gpu = null, disk = null, net = null;
      if (etwRates is not null && etwRates.TryGetValue(pid, out var r)) {
        gpu = r.GpuPercent;
        disk = r.DiskBytesPerSec;
        net = r.NetBytesPerSec;
      } else if (etwRates is not null) {
        // ETW is running but saw no activity for this PID this window — that's a real zero, not
        // "unwired". Show 0 rather than the em-dash placeholder.
        gpu = 0; disk = 0; net = 0;
      }

      // Session 0 is the non-interactive services session → Windows infrastructure. Anything in an
      // interactive session with a visible window is an App; the rest are background processes.
      ProcessCategory category =
          m.SessionId == 0 ? ProcessCategory.WindowsProcess
          : windowedPids.Contains(pid) ? ProcessCategory.App
          : ProcessCategory.BackgroundProcess;

      samples.Add(new ProcessSample(
          ProcessId: pid,
          Name: m.Name ?? "(unknown)",
          CpuPercent: cpuPercent,
          WorkingSetMb: m.WorkingSetInMB ?? 0,
          Category: category,
          // Win32_Process.Status is almost always empty; fall back to "Running" so the
          // column reads sensibly instead of showing the em-dash placeholder everywhere.
          Status: string.IsNullOrWhiteSpace(m.Status) ? "Running" : m.Status,
          GpuPercent: gpu,
          DiskBytesPerSec: disk,
          NetBytesPerSec: net));
    }

    // Replace the baseline so exited processes don't linger and skew the next diff.
    _lastCpuTime.Clear();
    foreach (var (pid, time) in seenCpuTime) _lastCpuTime[pid] = time;

    return samples;
  }
}
