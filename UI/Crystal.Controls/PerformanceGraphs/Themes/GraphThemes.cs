using Crystal.Controls.PerformanceGraphs.Kinds;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Themes;

/// <summary>Built-in <see cref="GraphTheme"/> presets for <see cref="PerformanceGraph"/>.</summary>
public static class GraphThemes {
  private static readonly Brush DefaultBackground = Freeze(Brushes.Black);
  private static readonly Brush DefaultGrid = Freeze(new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30)));
  private static readonly Brush DefaultBorder = Freeze(new SolidColorBrush(Color.FromRgb(0x3E, 0x7B, 0xC4)));

  /// <summary>Rose/magenta accent — matches a typical "% Utilization" graph.</summary>
  public static GraphTheme Rose(GraphKind kind = GraphKind.Line) => FromAccent(Color.FromRgb(0xE8, 0x2A, 0x7A), kind);

  /// <summary>Emerald/green accent — matches a typical "Voltage" or "healthy" graph.</summary>
  public static GraphTheme Emerald(GraphKind kind = GraphKind.Line) => FromAccent(Color.FromRgb(0x3B, 0xD1, 0x5A), kind);

  /// <summary>Amber accent — a common "warning" color.</summary>
  public static GraphTheme Amber(GraphKind kind = GraphKind.Line) => FromAccent(Color.FromRgb(0xE8, 0x9B, 0x2A), kind);

  /// <summary>Sky-blue accent.</summary>
  public static GraphTheme Sky(GraphKind kind = GraphKind.Line) => FromAccent(Color.FromRgb(0x3E, 0x9B, 0xE8), kind);

  /// <summary>
  /// Builds a theme from a single accent color: the line and fill both derive from it, over a
  /// shared dark background/grid/border.
  ///
  /// The fill differs by <paramref name="kind"/>, and this is the important part, not just a
  /// detail: for <see cref="GraphKind.Line"/> the fill is a vertical gradient (solid-ish at the
  /// line, fading to transparent at the baseline) because FilledLineRenderer draws one
  /// continuous shape, so the gradient reads as a single smooth glow. For
  /// <see cref="GraphKind.Bar"/> and <see cref="GraphKind.SegmentedBar"/> the fill is a flat
  /// SolidColorBrush instead — BarRenderer/SegmentedBarRenderer draw many separate rectangles,
  /// and WPF's default relative gradient mapping restarts within each one it's used on, so the
  /// same "glow" gradient would instead repeat per bar (or per segment, which looks especially
  /// broken — every little block fading independently rather than one continuous fade up the
  /// stack). A flat fill is what actually reads correctly once the data is drawn as discrete
  /// pieces.
  /// </summary>
  public static GraphTheme FromAccent(Color accent, GraphKind kind = GraphKind.Line) {
    return new GraphTheme {
      GraphBackground = DefaultBackground,
      GridBrush = DefaultGrid,
      BorderBrush = DefaultBorder,
      LineBrush = Freeze(new SolidColorBrush(accent)),
      LineThickness = 1.5,
      FillBrush = kind == GraphKind.Line ? CreateVerticalGlow(accent) : Freeze(new SolidColorBrush(accent))
    };
  }

  private static Brush CreateVerticalGlow(Color accent) {
    // The gradient is mapped to the full plot height (bright near MaxValue, faint at the
    // baseline). A low reading only exposes the bottom of that gradient, so the fill must stay
    // opaque enough there to read clearly — hence a solid floor rather than fading to fully
    // transparent, which left sub-10% values with no visible fill at all.
    var brush = new LinearGradientBrush {
      StartPoint = new Point(0, 0),
      EndPoint = new Point(0, 1)
    };
    brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xC0, accent.R, accent.G, accent.B), 0));
    brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x50, accent.R, accent.G, accent.B), 1));
    return Freeze(brush);
  }

  private static Brush Freeze(Brush brush) {
    if (brush.CanFreeze) brush.Freeze();
    return brush;
  }
}
