using Crystal.Provider.Etw;
using System.Diagnostics;
using System.Reactive.Linq;

namespace Crystal.Service.Network;

/// <summary>
/// Turns the shared <see cref="EtwRateBroadcaster"/> stream into a ranked per-process network
/// top-talkers list. Each broadcast carries per-PID ETW rates (combined send + receive); we keep the
/// busiest processes, resolve their names, and emit a <see cref="ProcessNetworkSnapshot"/>. The
/// broadcaster owns the single destructive SnapshotRates() poll, so subscribing here does not steal
/// the process list's window — both read the same snapshot.
/// </summary>
public sealed class ProcessNetworkSource {
  /// <summary>
  /// How many top talkers to keep per poll; a dense table wants a short list — more is noise on a monitoring tile.
  /// </summary>
  private const int TopCount = 8;

  /// <summary>
  /// Shared source of per-PID ETW network rates; owns the single destructive rate poll.
  /// </summary>
  private readonly EtwRateBroadcaster _broadcaster;

  /// <summary>
  /// Resolves PIDs to process names, invoked once per poll. Injectable for tests.
  /// </summary>
  private readonly Func<IReadOnlyDictionary<uint, string>> _nameResolver;

  /// <summary>
  /// Captures the shared broadcaster and the PID-to-name resolver. When no resolver is supplied,
  /// <see cref="DefaultNameResolver"/> (a live <see cref="Process"/> enumeration) is used.
  /// </summary>
  /// <param name="broadcaster">Shared ETW rate source subscribed to for per-process throughput.</param>
  /// <param name="nameResolver">Optional PID-to-name resolver; defaults to a live process enumeration.</param>
  public ProcessNetworkSource(EtwRateBroadcaster broadcaster,
                              Func<IReadOnlyDictionary<uint, string>>? nameResolver = null) {
    ArgumentNullException.ThrowIfNull(broadcaster);
    _broadcaster = broadcaster;
    _nameResolver = nameResolver ?? DefaultNameResolver;
  }

  /// <summary>
  /// Ranked top-talkers, one emission per broadcaster poll while subscribed.
  /// </summary>
  public IObservable<ProcessNetworkSnapshot> TopTalkers =>
      _broadcaster.Rates.Select(BuildSnapshot);

  /// <summary>
  /// Projects one broadcaster emission into a ranked snapshot: drops idle PIDs, sorts by throughput,
  /// takes the top <see cref="TopCount"/>, and resolves names. An empty rate map yields an empty
  /// snapshot carrying the broadcaster's running/error state so the UI can explain a blank table.
  /// </summary>
  /// <param name="rates">Per-PID ETW metrics for this poll window.</param>
  /// <returns>The ranked top-talkers snapshot for this poll.</returns>
  private ProcessNetworkSnapshot BuildSnapshot(IReadOnlyDictionary<uint, ProcessEtwMetrics> rates) {
    if (rates.Count == 0)
      return new ProcessNetworkSnapshot([], _broadcaster.IsRunning, _broadcaster.StartError);

    // Resolve names once per poll rather than per PID: a single process snapshot is far cheaper than
    // GetProcessById in a loop, and avoids a throw for every PID that exited mid-window.
    var names = _nameResolver();

    var talkers = rates
        .Where(kv => kv.Value.NetBytesPerSec > 0)
        .OrderByDescending(kv => kv.Value.NetBytesPerSec)
        .Take(TopCount)
        .Select(kv => new ProcessNetworkReading(
            ProcessId: kv.Key,
            Name: names.TryGetValue(kv.Key, out var n) ? n : $"PID {kv.Key}",
            NetBytesPerSecond: kv.Value.NetBytesPerSec))
        .ToList();

    return new ProcessNetworkSnapshot(talkers, _broadcaster.IsRunning, _broadcaster.StartError);
  }

  /// <summary>
  /// Builds a PID-to-name map from a single live process enumeration. A process can exit between
  /// enumeration and read, so per-process failures are swallowed (the "PID {id}" fallback covers
  /// them) and every handle is disposed.
  /// </summary>
  /// <returns>A snapshot map of PID to process name.</returns>
  private static IReadOnlyDictionary<uint, string> DefaultNameResolver() {
    var map = new Dictionary<uint, string>();
    foreach (var p in Process.GetProcesses()) {
      try {
        map[(uint)p.Id] = p.ProcessName;
      } catch {
        // A process can exit between enumeration and read; skip it — the fallback name covers it.
      } finally {
        p.Dispose();
      }
    }
    return map;
  }
}
