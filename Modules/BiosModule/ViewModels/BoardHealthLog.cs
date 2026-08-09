using System;
using System.Collections.Generic;
using System.Linq;

namespace BiosModule.ViewModels;

/// <summary>One out-of-spec episode for a single board sensor: when it first went out of tolerance,
/// how long it lasted, the worst severity reached over its lifetime, and the formatted reading
/// captured at the moment it first reached that worst severity (e.g. "10.4 V"). <see cref="EndedAt"/>
/// is null while the sensor is still out of spec (an ongoing episode).</summary>
public sealed record BoardHealthEvent(
    string SensorName,
    ReadingSeverity Severity,
    string PeakValue,
    DateTimeOffset StartedAt,
    DateTimeOffset PeakAt,
    DateTimeOffset? EndedAt) {
  public bool Ongoing => EndedAt is null;
}

/// <summary>Session-scoped record of board-health episodes. Each tick is handed the sensors that are
/// currently out of spec; the log opens an episode the first tick a sensor appears, escalates its
/// worst severity while it stays out of spec, and closes it (stamping <see cref="BoardHealthEvent.EndedAt"/>)
/// the tick it recovers. A sensor that goes out of spec again later opens a fresh episode, so a
/// flapping rail reads as several short events rather than one long one — that distinction is the
/// whole point of persisting the history rather than only showing the live severity.</summary>
internal sealed class BoardHealthLog {
  // Bounds how many recovered episodes are retained; ongoing ones are naturally bounded by the
  // sensor count. The tail is dropped oldest-first so a long noisy session can't grow without limit.
  private const int MaxClosedEpisodes = 50;

  private sealed class OpenEpisode {
    public required string SensorName;
    public ReadingSeverity Worst;
    // Formatted reading captured the first tick the episode reached its Worst severity, so the log
    // shows the number that triggered the peak rather than whatever the sensor happens to read now.
    public string PeakValue = "";
    // When the episode first reached its Worst severity — pinned alongside PeakValue so the log can
    // show *when* the peak happened, not just when the episode opened.
    public DateTimeOffset PeakAt;
    public DateTimeOffset StartedAt;
    public DateTimeOffset LastSeenAt;
  }

  private readonly Func<DateTimeOffset> _clock;
  private readonly Dictionary<string, OpenEpisode> _open = [];
  private readonly List<BoardHealthEvent> _closed = [];
  // How many recovered episodes have been evicted by the cap this session, so the view can say the
  // trail is truncated rather than pretending the oldest retained event is the session's first.
  private int _droppedCount;

  public BoardHealthLog(Func<DateTimeOffset> clock) => _clock = clock;

  /// <summary>Count of recovered episodes dropped by the retention cap this session (0 until the
  /// cap is first exceeded), so the detail view can flag that the oldest history is no longer shown.</summary>
  public int DroppedCount => _droppedCount;

  /// <summary>Discards all episodes — ongoing and recovered — so a fresh observation window can
  /// start without restarting the app. An out-of-spec sensor simply opens a new episode next tick.</summary>
  public void Clear() {
    _open.Clear();
    _closed.Clear();
    _droppedCount = 0;
  }

  /// <summary>Folds the tick's out-of-spec sensors into the log: opens episodes for newly-offending
  /// sensors, escalates the worst severity of ones already open, and closes any open episode whose
  /// sensor is no longer in the list. <c>Value</c> is the formatted live reading (e.g. "10.4 V")
  /// captured as the episode's peak the first tick it reaches its worst severity.</summary>
  public void Observe(IReadOnlyList<(string Name, ReadingSeverity Severity, string Value)> offenders) {
    var now = _clock();
    var seen = new HashSet<string>(offenders.Count);

    foreach (var (name, severity, value) in offenders) {
      seen.Add(name);
      if (_open.TryGetValue(name, out var episode)) {
        // Capture the reading only when the episode first reaches a new worst band, so the peak
        // value stays pinned to that moment rather than tracking every later in-band wobble.
        if (severity > episode.Worst) {
          episode.Worst = severity;
          episode.PeakValue = value;
          episode.PeakAt = now;
        }
        episode.LastSeenAt = now;
      }
      else {
        _open[name] = new OpenEpisode {
          SensorName = name, Worst = severity, PeakValue = value,
          StartedAt = now, PeakAt = now, LastSeenAt = now,
        };
      }
    }

    // Any open episode not seen this tick has recovered — close it, dating the end to when it was
    // last still out of spec, not now, so a sensor that recovered several ticks ago isn't credited
    // with the idle time in between.
    foreach (var name in _open.Keys.ToList()) {
      if (seen.Contains(name)) continue;
      var e = _open[name];
      _open.Remove(name);
      Append(new BoardHealthEvent(e.SensorName, e.Worst, e.PeakValue, e.StartedAt, e.PeakAt, e.LastSeenAt));
    }
  }

  /// <summary>Every episode, ongoing ones first (newest start first), then recovered ones (newest
  /// end first) — so the detail view shows current faults at the top and recent history below.</summary>
  public IReadOnlyList<BoardHealthEvent> Snapshot() {
    var ongoing = _open.Values
        .Select(e => new BoardHealthEvent(e.SensorName, e.Worst, e.PeakValue, e.StartedAt, e.PeakAt, null))
        .OrderByDescending(e => e.StartedAt);
    var recovered = _closed
        .OrderByDescending(e => e.EndedAt);
    return ongoing.Concat(recovered).ToList();
  }

  private void Append(BoardHealthEvent e) {
    _closed.Add(e);
    if (_closed.Count > MaxClosedEpisodes) {
      _closed.RemoveAt(0);
      _droppedCount++;
    }
  }
}
