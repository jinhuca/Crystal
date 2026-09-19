namespace Crystal.Controls.PerformanceGraphs.Kinds;

/// <summary>Selects how a <see cref="PerformanceGraph"/> colors its <see cref="DisplayMode.Dot"/> dots.</summary>
public enum DotColorMode {
  /// <summary>Every dot uses <see cref="PerformanceGraph.DotColor"/>. Renders through one
  /// reused <see cref="System.Windows.Media.StreamGeometry"/> and exactly one
  /// <see cref="System.Windows.Media.DrawingContext.DrawGeometry"/> call per frame - no per-row
  /// band lookup, and the nine band geometries/brushes used by <see cref="Banded"/> mode are never
  /// allocated or touched.</summary>
  SingleColor,

  /// <summary>Each dot is colored by which of nine fixed value-bands its row falls in (see
  /// <see cref="PerformanceGraph.Color1"/> through <see cref="PerformanceGraph.Color9"/>).
  /// Batches into up to nine reused geometries, one per band actually lit this frame. This is the
  /// default, so an unconfigured graph keeps its original green→red gauge look.</summary>
  Banded
}
