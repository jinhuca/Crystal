namespace Crystal.Controls.Metrics;

/// <summary>
/// Short-term direction of a live metric relative to its own smoothed baseline: the trend cue a
/// sparkline used to convey. Computed cheaply per sample (no rendering), so a graph-less readout can
/// still show whether a value is climbing, falling, or holding steady.
/// </summary>
public enum MetricTrend {
  /// <summary>Holding within the deadband of its smoothed baseline (no meaningful movement).</summary>
  Flat,

  /// <summary>Reading is above its smoothed baseline by more than the deadband.</summary>
  Rising,

  /// <summary>Reading is below its smoothed baseline by more than the deadband.</summary>
  Falling,
}
