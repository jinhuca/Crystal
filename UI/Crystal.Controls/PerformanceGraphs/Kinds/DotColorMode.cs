namespace Crystal.Controls.PerformanceGraphs.Kinds;

/// <summary>Selects how <see cref="PerformanceGraphLite"/> colors its dots.</summary>
public enum DotColorMode {
  /// <summary>Every dot uses <see cref="PerformanceGraphLite.DotColor"/>. Renders through one
  /// reused <see cref="System.Windows.Media.StreamGeometry"/> and exactly one
  /// <see cref="System.Windows.Media.DrawingContext.DrawGeometry"/> call per frame - no per-row
  /// band lookup, and the nine band geometries/brushes used by <see cref="Banded"/> mode are never
  /// allocated or touched.</summary>
  SingleColor,

  /// <summary>Each dot is colored by which of nine fixed value-bands its row falls in (see
  /// <see cref="PerformanceGraphLite.Color1"/> through <see cref="PerformanceGraphLite.Color9"/>).
  /// Batches into up to nine reused geometries, one per band actually lit this frame. This is the
  /// default, so an unconfigured graph keeps its original green→red gauge look.</summary>
  Banded
}
