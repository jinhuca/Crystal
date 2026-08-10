namespace Crystal.Provider.Etw;

/// <summary>Per-process rates derived from ETW over the interval since the previous snapshot.</summary>
public sealed record ProcessEtwMetrics(
    double DiskBytesPerSec,
    double NetBytesPerSec,
    double GpuPercent);

/// <summary>
/// A real-time source of per-process GPU / disk / network activity backed by a kernel ETW session.
/// Implementations run a background trace session (which requires the process to be elevated) and
/// expose an atomic rate snapshot keyed by PID.
/// </summary>
public interface IProcessEtwSource : IDisposable {
  /// <summary>True once a kernel session is running; false if it could not start (e.g. not elevated).</summary>
  bool IsRunning { get; }

  /// <summary>
  /// Null while running or before start is attempted; otherwise a short human-readable reason the
  /// kernel session did not start (e.g. "not elevated", or the underlying exception message). Lets
  /// the UI explain why per-process GPU/Disk/Network are blank instead of failing silently.
  /// </summary>
  string? StartError { get; }

  /// <summary>
  /// Returns per-PID rates covering the window since the previous call, then resets the interval
  /// accumulators. Call on a steady cadence (the process poll cadence). Empty if not running.
  /// </summary>
  IReadOnlyDictionary<uint, ProcessEtwMetrics> SnapshotRates();

  /// <summary>
  /// Suspends per-event accumulation without tearing down the kernel session. Call when the window
  /// is minimized/hidden — nobody is watching the per-process GPU/Disk/Network columns, so skipping
  /// the (otherwise continuous) per-event parsing and bookkeeping saves idle CPU. Idempotent; a
  /// no-op when the session never started. Snapshots return empty while paused.
  /// </summary>
  void Pause();

  /// <summary>
  /// Resumes accumulation after <see cref="Pause"/>, starting a fresh rate window (the paused
  /// interval is discarded rather than reported as one long spike). Idempotent; a no-op when the
  /// session never started.
  /// </summary>
  void Resume();
}
