using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System.Diagnostics;
using System.Security.Principal;

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

  // Stable session name (not PID-suffixed): a kernel real-time session survives a process crash,
  // so reusing the same name lets a fresh run find and stop the orphan before recreating it.
  private const string SessionName = "CrystalProcessEtw";

  private TraceEventSession? _session;
  private Thread? _processingThread;
  private readonly Stopwatch _clock = Stopwatch.StartNew();
  private double _lastSnapshotMs;
  private volatile bool _running;
  // When true, event handlers return immediately: the kernel session stays up but stops feeding the
  // accumulators, so a minimized/hidden window costs almost nothing. Read on the pump thread (per
  // event) and on the GPU-event thread; written from the UI thread — volatile suffices since it only
  // gates work, and a snapshot clears the accumulators under the lock anyway.
  private volatile bool _paused;
  private string? _startError;
  private bool _disposed;

  private sealed class Accumulator {
    public double DiskBytes;
    public double NetBytes;
    public double GpuBusyMs;
  }

  public bool IsRunning => _running;
  public string? StartError => _startError;

  public ProcessEtwReader() {
    if (!IsElevated()) {
      // A real-time kernel session requires administrator rights; skip the attempt and say why,
      // rather than throwing a COMException we'd have to swallow.
      _startError = "not elevated (a real-time kernel ETW session requires administrator rights)";
      _running = false;
      return;
    }

    try {
      Start();
    } catch (Exception ex) {
      // Stay inert rather than crash the app, but record the reason so the failure isn't silent.
      // The usual culprit is a leftover kernel session (COMException) or lost elevation.
      _startError = ex.Message;
      _running = false;
      try { _session?.Dispose(); } catch { /* best effort */ }
      _session = null;
    }
  }

  private static bool IsElevated() {
    try {
      using var identity = WindowsIdentity.GetCurrent();
      return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    } catch {
      // If we can't determine it, let Start() try and surface any real failure.
      return true;
    }
  }

  private void Start() {
    // A kernel real-time session outlives the process that created it, so a prior hard-stop (crash,
    // debugger "Stop", kill) can leave an orphan holding the name and the kernel logger slot. That
    // orphan is exactly what makes TraceEventSession's ctor throw on the next launch. Stop any
    // same-named leftover first so startup is self-healing instead of one-shot.
    StopStaleSession();

    // DxgKrnl's manifest has an event template TraceEvent can't fully lay out; enabling it spews a
    // benign "Array is variable sized..." line to System.Diagnostics.Trace. Swallow just that line.
    TraceEventNoiseFilter.Install();

    _session = new TraceEventSession(SessionName) {
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

  private static void StopStaleSession() {
    try {
      // GetActiveSessionNames enumerates every logger currently registered with the kernel, ours
      // included if a prior run didn't shut down cleanly. Attaching to a named session and disposing
      // it (StopOnDispose) is how TraceEvent stops an orphan we no longer hold a handle to.
      if (!TraceEventSession.GetActiveSessionNames().Contains(SessionName)) return;
      using var stale = new TraceEventSession(SessionName, TraceEventSessionOptions.Attach) {
        StopOnDispose = true,
      };
    } catch {
      // Best effort: if we can't attach/stop it, the ctor below will surface the real failure.
    }
  }

  private void WireKernelEvents(KernelTraceEventParser kernel) {
    kernel.DiskIORead += e => Add(a => a.DiskBytes += e.TransferSize, (uint)e.ProcessID);
    kernel.DiskIOWrite += e => Add(a => a.DiskBytes += e.TransferSize, (uint)e.ProcessID);

    kernel.TcpIpSend += e => Add(a => a.NetBytes += e.size, (uint)e.ProcessID);
    kernel.TcpIpRecv += e => Add(a => a.NetBytes += e.size, (uint)e.ProcessID);
    kernel.UdpIpSend += e => Add(a => a.NetBytes += e.size, (uint)e.ProcessID);
    kernel.UdpIpRecv += e => Add(a => a.NetBytes += e.size, (uint)e.ProcessID);
  }

  // How a DxgKrnl event maps onto DMA-packet timing, decided once per distinct event name.
  internal enum DmaKind { Ignore, Start, Stop }

  // dynamic.All fires for every DxgKrnl event — the highest-frequency stream we subscribe to. The
  // old handler ran up to four case-insensitive IndexOf scans over the event name on every single
  // event; classifying each distinct name once and caching the verdict turns the steady-state cost
  // into a dictionary lookup. All DynamicTraceEventParser callbacks run serially on the one pump
  // thread, so this cache needs no synchronization.
  private readonly Dictionary<string, DmaKind> _dmaKindByName = new(StringComparer.Ordinal);

  internal static DmaKind ClassifyDma(string name) {
    if (name.IndexOf("DmaPacket", StringComparison.OrdinalIgnoreCase) < 0) return DmaKind.Ignore;
    if (name.IndexOf("Start", StringComparison.OrdinalIgnoreCase) >= 0) return DmaKind.Start;
    if (name.IndexOf("Stop", StringComparison.OrdinalIgnoreCase) >= 0
        || name.IndexOf("Info", StringComparison.OrdinalIgnoreCase) >= 0) return DmaKind.Stop;
    return DmaKind.Ignore;
  }

  // DxgKrnl DMA packets bracket GPU execution: a Start carries a context+packet id, the matching
  // Stop marks completion. We attribute the elapsed wall time between the pair to the emitting
  // process. This is an approximation of engine-busy time, not exact hardware occupancy, but it
  // tracks Task Manager's "GPU %" closely enough for a per-process ranking.
  private void WireGpuEvents(DynamicTraceEventParser dynamic) {
    dynamic.All += e => {
      if (_paused) return;

      // e.EventName allocates/formats the name; look up the cached verdict by it and only fall back
      // to the (one-time) string scan when we've not seen this event name before.
      var name = e.EventName;
      if (!_dmaKindByName.TryGetValue(name, out var kind)) {
        kind = ClassifyDma(name);
        _dmaKindByName[name] = kind;
      }
      if (kind == DmaKind.Ignore) return;

      // Key each in-flight packet by the context+packet id fields the DxgKrnl payload exposes.
      ulong key = PacketKey(e);
      double nowMs = e.TimeStampRelativeMSec;

      if (kind == DmaKind.Start) {
        lock (_gate) _dmaStartMs[key] = nowMs;
      } else {
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
    if (_paused || pid == 0) return;
    lock (_gate) mutate(GetLocked(pid));
  }

  private Accumulator GetLocked(uint pid) {
    if (!_acc.TryGetValue(pid, out var a)) {
      a = new Accumulator();
      _acc[pid] = a;
    }
    return a;
  }

  public void Pause() {
    if (!_running) return;
    _paused = true;
    // Drop whatever accumulated up to the pause so it isn't reported after resume. Handlers already
    // early-out on _paused, but one could be mid-flight, so clear under the lock.
    lock (_gate) {
      _acc.Clear();
      _dmaStartMs.Clear();
    }
  }

  public void Resume() {
    if (!_running || !_paused) return;
    // Start a fresh window: without resetting the snapshot clock, the first post-resume snapshot
    // would divide by the whole paused gap and read as a near-zero rate for real activity.
    lock (_gate) {
      _acc.Clear();
      _dmaStartMs.Clear();
      _lastSnapshotMs = _clock.Elapsed.TotalMilliseconds;
    }
    _paused = false;
  }

  public IReadOnlyDictionary<uint, ProcessEtwMetrics> SnapshotRates() {
    if (!_running || _paused) return EmptySnapshot;

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
