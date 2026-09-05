namespace Crystal.Shell.Settings;

/// <summary>
/// The bar shape for the CPU per-core meter strip: a solid <see cref="Bar"/> or a discrete
/// <see cref="SegmentedBar"/> (LED-meter), applied globally to all three core bars.
/// </summary>
public enum CoreBarStyle {
  Bar,
  SegmentedBar,
}

/// <summary>
/// The colouring for the CPU per-core meter strip: <see cref="Colorful"/> keeps the distinct
/// per-metric colours (clock/load/temp); <see cref="Grey"/> paints every bar a uniform muted grey.
/// </summary>
public enum CoreBarColor {
  Colorful,
  Grey,
}

/// <summary>
/// The factory-reset core-strip appearance.
/// </summary>
public static class GraphDefaults {
  public const CoreBarStyle CoreBarStyle = Settings.CoreBarStyle.SegmentedBar;
  public const CoreBarColor CoreBarColor = Settings.CoreBarColor.Colorful;
}

/// <summary>
/// The full persisted graph-appearance selection: the global render mode shared by every dashboard
/// graph, plus the CPU core-strip look.
/// </summary>
public sealed class GraphSettings {
  // Global render mode for every dashboard graph: a filled line or a dot-matrix gauge. Written by the
  // title-bar Line/Dot toggle and mirrored onto Crystal.Controls' GraphAppearance singleton so the
  // choice is applied to every graph at once and restored on the next launch. A file that predates
  // this field keeps Line because an absent JSON property leaves the initializer value untouched.
  public Crystal.Controls.PerformanceGraphs.GraphRenderMode RenderMode { get; set; } =
      Crystal.Controls.PerformanceGraphs.GraphRenderMode.Line;

  // Global CPU core-strip look. A file that predates these fields keeps the defaults because absent
  // JSON properties leave the initializer values untouched.
  public CoreBarStyle CoreBarStyle { get; set; } = GraphDefaults.CoreBarStyle;
  public CoreBarColor CoreBarColor { get; set; } = GraphDefaults.CoreBarColor;
}
