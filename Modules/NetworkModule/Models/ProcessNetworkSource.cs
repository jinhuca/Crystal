using System.Diagnostics;
using System.Reactive.Linq;
using Crystal.Provider.Etw;

namespace NetworkModule.Models;

/// <summary>
/// Turns the shared <see cref="EtwRateBroadcaster"/> stream into a ranked per-process network
/// top-talkers list. Each broadcast carries per-PID ETW rates (combined send + receive); we keep the
/// busiest processes, resolve their names, and emit a <see cref="ProcessNetworkSnapshot"/>. The
/// broadcaster owns the single destructive SnapshotRates() poll, so subscribing here does not steal
/// the process list's window — both read the same snapshot.
/// </summary>
public sealed class ProcessNetworkSource {
  // A dense table wants a short list; more than this is noise on a monitoring tile.
  private const int TopCount = 8;

  private readonly EtwRateBroadcaster _broadcaster;
  private readonly Func<IReadOnlyDictionary<uint, string>> _nameResolver;

  public ProcessNetworkSource(EtwRateBroadcaster broadcaster,
                              Func<IReadOnlyDictionary<uint, string>>? nameResolver = null) {
    ArgumentNullException.ThrowIfNull(broadcaster);
    _broadcaster = broadcaster;
    _nameResolver = nameResolver ?? DefaultNameResolver;
  }

  /// <summary>Ranked top-talkers, one emission per broadcaster poll while subscribed.</summary>
  public IObservable<ProcessNetworkSnapshot> TopTalkers =>
      _broadcaster.Rates.Select(BuildSnapshot);

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
