namespace Crystal.Controls.PerformanceGraphs;

/// <summary>
/// How a <see cref="PerformanceGraph"/> draws its buffered samples. Replaces the older
/// <c>GraphKind</c>/separate-control split: one control now renders all three, so a graph can be
/// switched between them at runtime (the mode is an <c>AffectsRender</c> property, so the existing
/// sample buffer is kept and simply re-drawn).
/// </summary>
public enum DisplayMode {
  /// <summary>A single continuous filled line/area for the primary series, plus any overlay series
  /// added via <see cref="PerformanceGraph.AddSeries"/>.</summary>
  Line,

  /// <summary>A dot-matrix gauge: each column of samples drawn as a stack of dots, value-banded
  /// (green→red) or a single flat color.</summary>
  Dot,

  /// <summary>Multiple independent lines, one per <see cref="DataSeries"/> in
  /// <see cref="PerformanceGraph.Series"/>, sharing one set of axes.</summary>
  MultipleLine,
}
