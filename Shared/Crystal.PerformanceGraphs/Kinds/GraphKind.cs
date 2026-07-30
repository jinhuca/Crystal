namespace Crystal.PerformanceGraphs.Kinds;

/// <summary>Selects how a <see cref="PerformanceGraph"/> renders its buffered sample data.</summary>
public enum GraphKind {
  /// <summary>A continuous filled line/area, like a Task-Manager utilization graph.</summary>
  Line,

  /// <summary>Discrete bars, one per sample.</summary>
  Bar,

  /// <summary>Discrete bars made of stacked LED-style segments, one per sample.</summary>
  SegmentedBar
}
