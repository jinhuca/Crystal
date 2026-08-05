using System.Diagnostics;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;

namespace Crystal.Provider.Etw;

/// <summary>
/// Runs a real-time kernel ETW session and continuously accumulates per-process disk bytes,
/// network bytes and GPU busy time on a background thread. <see cref="SnapshotRates"/> converts
/// the accumulated totals into per-second rates over the elapsed interval and clears them.
/// <para>
/// A real-time kernel session requires the process to run elevated; if the session can't start
/// (typically because it isn't), the reader stays inert with <see cref="IsRunning"/> false and
/// hands back empty snapshots so the rest of the app degrades gracefully.
/// </para>
/// </summary>
public sealed class ProcessEtwReader : IProcessEtwSource {
  // Microsoft-Windows-DxgKrnl — the kernel graphics scheduler. Its packet events carry the
  // per-context GPU execution timing Task Manager sums into "GPU %".
  private static readonly Guid DxgKrnlProvider = new("802ec45a-1e99-4b83-9920-87c98277ba9d");
  private const ulong DxgKrnlPerformanceKeyword = 0x1; // "Performance" — DMA packet start/stop.

  private readonly object _gate = new();
  private readonly Dictionary<uint, Accumulator> _acc = new();
  // Per-GPU-context in-flight DMA packet start times, to pair with their stop event.
  private readonly Dictionary<ulong, double> _dmaStartMs = new();

  private TraceEventSession? _session;
  private Thread? _processingThread;
  private readonly Stopwatch _clock = Stopwatch.StartNew();
  private double _lastSnapshotMs;
  private volatile bool _running;
  private bool _disposed;

  private sealed class Accumulator {
    public double DiskBytes;
    public double NetBytes;
    public double GpuBusyMs;
  }

  public bool IsRunning => _running;

  public ProcessEtwReader() {
    try {
      Start();
    } catch {
      // Most commonly UnauthorizedAccess when not elevated. Stay inert rather than crash the app.
      _running = false;
    }
  }

  private void Start() {
    // A unique-per-run session name avoids colliding with a leftover session from a prior crash.
    _session = new TraceEventSession("CrystalProcessEtw-" + Environment.ProcessId) {
      StopOnDispose = true,
    };

    _session.EnableKernelProvider(
        KernelTraceEventParser.Keywords.DiskIO |
        KernelTraceEventParser.Keywords.NetworkTCPIP);

    _session.EnableProvider(DxgKrnlProvider, TraceEventLevel.Informational, DxgKrnlPerformanceKeyword);

    WireKernelEvents(_session.Source.Kernel);
    WireGpuEvents(_session.Source.Dynamic);

    _lastSnapshotMs = _clock.Elapsed.TotalMilliseconds;

    // Source.Process() blocks until the session stops, so pump it on a dedicated background thread.
    _processingThread = new Thread(() => {
      try { _session.Source.Process(); } catch { /* session torn down */ }
    }) { IsBackground = true, Name = "CrystalEtwPump" };
    _processingThread.Start();
    _running = true;
  }

  private void WireKernelEvents(KernelTraceEventParser kernel) {
    kernel.DiskIORead += e => Add(a => a.DiskBytes += e.TransferSize, (uint)e.ProcessID);
    kernel.DiskIOWrite += e => Add(a => a.DiskBytes += e.TransferSize, (uint)e.ProcessID);

    kernel.TcpIpSend += e => Add(a => a.NetBytes += e.size, (uint)e.ProcessID);
    kernel.TcpIpRecv += e => Add(a => a.NetBytes += e.size, (uint)e.ProcessID);
    kernel.UdpIpSend += e => Add(a => a.NetBytes += e.size, (uint)e.ProcessID);
    kernel.UdpIpRecv += e => Add(a => a.NetBytes += e.size, (uint)e.ProcessID);
  }

  // DxgKrnl DMA packets bracket GPU execution: a Start carries a context+packet id, the matching
  // Stop marks completion. We attribute the elapsed wall time between the pair to the emitting
  // process. This is an approximation of engine-busy time, not exact hardware occupancy, but it
  // tracks Task Manager's "GPU %" closely enough for a per-process ranking.
  private void WireGpuEvents(DynamicTraceEventParser dynamic) {
    dynamic.All += e => {
      var name = e.EventName;
      if (name.IndexOf("DmaPacket", StringComparison.OrdinalIgnoreCase) < 0) return;

      // Key each in-flight packet by the context+packet id fields the DxgKrnl payload exposes.
      ulong key = PacketKey(e);
      double nowMs = e.TimeStampRelativeMSec;

      if (name.IndexOf("Start", StringComparison.OrdinalIgnoreCase) >= 0) {
        lock (_gate) _dmaStartMs[key] = nowMs;
      } else if (name.IndexOf("Stop", StringComparison.OrdinalIgnoreCase) >= 0
                 || name.IndexOf("Info", StringComparison.OrdinalIgnoreCase) >= 0) {
        lock (_gate) {
          if (_dmaStartMs.Remove(key, out var startMs) && nowMs >= startMs) {
            GetLocked((uint)e.ProcessID).GpuBusyMs += nowMs - startMs;
          }
        }
      }
    };
  }

  private static ulong PacketKey(TraceEvent e) {
    // hContext + SubmitSequence uniquely identify a packet within a run; fall back to any subset
    // that's present so a missing field still yields a stable-ish key rather than throwing.
    ulong context = TryPayloadULong(e, "hContext");
    ulong seq = TryPayloadULong(e, "SubmitSequence");
    return (context << 20) ^ seq;
  }

  private static ulong TryPayloadULong(TraceEvent e, string field) {
    try {
      var v = e.PayloadByName(field);
      return v is null ? 0 : System.Convert.ToUInt64(v);
    } catch {
      return 0;
    }
  }

  private void Add(Action<Accumulator> mutate, uint pid) {
    if (pid == 0) return;
    lock (_gate) mutate(GetLocked(pid));
  }

  private Accumulator GetLocked(uint pid) {
    if (!_acc.TryGetValue(pid, out var a)) {
      a = new Accumulator();
      _acc[pid] = a;
    }
    return a;
  }

  public IReadOnlyDictionary<uint, ProcessEtwMetrics> SnapshotRates() {
    if (!_running) return EmptySnapshot;

    lock (_gate) {
      double nowMs = _clock.Elapsed.TotalMilliseconds;
      double seconds = (nowMs - _lastSnapshotMs) / 1000.0;
      _lastSnapshotMs = nowMs;
      if (seconds <= 0) return EmptySnapshot;

      var result = new Dictionary<uint, ProcessEtwMetrics>(_acc.Count);
      foreach (var (pid, a) in _acc) {
        // GPU % is busy-time over wall-time; clamp since overlapping engines can exceed the window.
        double gpu = Math.Min(100.0, a.GpuBusyMs / (seconds * 1000.0) * 100.0);
        result[pid] = new ProcessEtwMetrics(
            DiskBytesPerSec: a.DiskBytes / seconds,
            NetBytesPerSec: a.NetBytes / seconds,
            GpuPercent: gpu);
      }

      // Reset interval accumulators; drop stale in-flight packets so a lost Stop can't leak.
      _acc.Clear();
      _dmaStartMs.Clear();
      return result;
    }
  }

  private static readonly IReadOnlyDictionary<uint, ProcessEtwMetrics> EmptySnapshot =
      new Dictionary<uint, ProcessEtwMetrics>();

  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _running = false;
    try { _session?.Dispose(); } catch { /* best effort */ }
    _session = null;
  }
}
