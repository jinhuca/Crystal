namespace Crystal.Controls.PerformanceGraphs;

/// <summary>
/// How every dashboard history graph draws its samples: a continuous filled line or a
/// dot-matrix gauge. Maps to a <see cref="PerformanceGraph"/>'s <see cref="DisplayMode"/>
/// (<see cref="DisplayMode.Line"/> / <see cref="DisplayMode.Dot"/>).
/// </summary>
public enum GraphRenderMode {
  /// <summary>
  /// Filled line — <see cref="PerformanceGraph"/> with <see cref="DisplayMode.Line"/>.
  /// </summary>
  Line,

  /// <summary>
  /// Dot-matrix gauge — <see cref="PerformanceGraph"/> with <see cref="DisplayMode.Dot"/>.
  /// </summary>
  Dot,
}
