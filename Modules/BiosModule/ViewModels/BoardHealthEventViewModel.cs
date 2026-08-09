using System;

namespace BiosModule.ViewModels;

/// <summary>One row in the detail view's board-health event log: a past or ongoing out-of-spec
/// episode, with its start time, worst severity, and human-readable duration.</summary>
public sealed class BoardHealthEventViewModel {
  public BoardHealthEventViewModel(BoardHealthEvent e, DateTimeOffset now) {
    SensorName = e.SensorName;
    Severity = e.Severity;
    PeakValue = e.PeakValue;
    Started = e.StartedAt.LocalDateTime.ToString("HH:mm:ss");
    StartedSort = e.StartedAt.UtcTicks;
    PeakAt = e.PeakAt.LocalDateTime.ToString("HH:mm:ss");
    PeakAtSort = e.PeakAt.UtcTicks;
    // Age counts from when the episode began, up to now — recomputed each rebuild so it ticks
    // forward. Clamped at zero so a clock that jumped backwards never shows a negative age.
    Age = FormatAge(now - e.StartedAt);
    var span = (e.EndedAt ?? now) - e.StartedAt;
    Duration = e.Ongoing ? $"ongoing · {FormatSpan(span)}" : FormatSpan(span);
    DurationSort = span.TotalSeconds;
    Ongoing = e.Ongoing;

    // Full detail for the row tooltip: the table shows abbreviated columns (HH:mm:ss start, coarse
    // duration), so hover surfaces the complete picture — full-date start, exact end or "ongoing",
    // peak reading and its band — without widening the grid.
    string ended = e.EndedAt is { } end
        ? end.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss")
        : "ongoing";
    Detail = string.Join(Environment.NewLine,
        $"{SensorName} — {Severity}",
        $"Peak reading: {PeakValue} at {e.PeakAt.LocalDateTime:HH:mm:ss}",
        $"Started: {e.StartedAt.LocalDateTime:yyyy-MM-dd HH:mm:ss}",
        $"Ended: {ended}",
        $"Duration: {Duration}");
  }

  public string SensorName { get; }
  public ReadingSeverity Severity { get; }
  /// <summary>The formatted reading at the episode's worst moment (e.g. "10.4 V"), so the log shows
  /// the number that triggered the peak severity, not just its band.</summary>
  public string PeakValue { get; }
  public string Started { get; }
  /// <summary>Clock time (HH:mm:ss) the episode first reached its peak severity — the moment the
  /// <see cref="PeakValue"/> reading was captured, which may be after the episode started.</summary>
  public string PeakAt { get; }
  /// <summary>How long ago the episode began, relative to now (e.g. "2m ago"), recomputed each
  /// rebuild so it ticks forward — a recency read that needs no arithmetic against the clock.</summary>
  public string Age { get; }
  public string Duration { get; }
  public bool Ongoing { get; }
  /// <summary>Multi-line full detail for the row's hover tooltip: full-date start/end, peak reading,
  /// and duration — the complete picture the abbreviated columns don't show.</summary>
  public string Detail { get; }

  /// <summary>Numeric keys behind the formatted Started/Duration strings, so column sorting is
  /// chronological / by-length rather than lexical (otherwise "9s" would sort after "10s", and the
  /// "ongoing · " prefix would dominate a text sort). <see cref="Severity"/> sorts directly on the
  /// enum (Normal &lt; Warning &lt; Critical), so descending puts the worst episodes first.</summary>
  public long StartedSort { get; }
  public long PeakAtSort { get; }
  public double DurationSort { get; }

  private static string FormatSpan(TimeSpan span) {
    if (span < TimeSpan.FromSeconds(1)) return "<1s";
    if (span < TimeSpan.FromMinutes(1)) return $"{(int)span.TotalSeconds}s";
    if (span < TimeSpan.FromHours(1)) return $"{(int)span.TotalMinutes}m {span.Seconds}s";
    return $"{(int)span.TotalHours}h {span.Minutes}m";
  }

  // Coarse "N ago" recency label. Unlike FormatSpan it drops the sub-unit remainder (minutes show
  // as "2m ago", not "2m 30s ago") so a value re-rendered every second doesn't flicker digit by
  // digit. Clamped so a backwards clock jump reads "just now" rather than a negative age.
  private static string FormatAge(TimeSpan span) {
    if (span < TimeSpan.Zero) return "just now";
    if (span < TimeSpan.FromSeconds(1)) return "<1s ago";
    if (span < TimeSpan.FromMinutes(1)) return $"{(int)span.TotalSeconds}s ago";
    if (span < TimeSpan.FromHours(1)) return $"{(int)span.TotalMinutes}m ago";
    return $"{(int)span.TotalHours}h {span.Minutes}m ago";
  }
}
