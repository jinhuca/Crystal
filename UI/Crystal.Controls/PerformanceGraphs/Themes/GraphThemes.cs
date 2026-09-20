using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Themes;

/// <summary>
/// Built-in <see cref="GraphTheme"/> presets for <see cref="PerformanceGraph"/>.
/// </summary>
public static class GraphThemes {
  /// <summary>
  /// The default background brush for the graph, which is black.
  /// </summary>
  private static readonly Brush DefaultBackground = Freeze(Brushes.Black);

  /// <summary>
  /// The default grid brush for the graph, which is a dark gray color.
  /// </summary>
  private static readonly Brush DefaultGrid = Freeze(new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30)));

  /// <summary>
  /// The default border brush for the graph, which is a blue color.
  /// </summary>
  private static readonly Brush DefaultBorder = Freeze(new SolidColorBrush(Color.FromRgb(0x3E, 0x7B, 0xC4)));

  /// <summary>
  /// Rose/magenta accent — matches a typical "% Utilization" graph.
  /// </summary>
  public static GraphTheme Rose(bool flatFill = false) => FromAccent(Color.FromRgb(0xE8, 0x2A, 0x7A), flatFill);

  /// <summary>
  /// Emerald/green accent — matches a typical "Voltage" or "healthy" graph.
  /// </summary>
  public static GraphTheme Emerald(bool flatFill = false) => FromAccent(Color.FromRgb(0x3B, 0xD1, 0x5A), flatFill);

  /// <summary>
  /// Amber accent — a common "warning" color.
  /// </summary>
  public static GraphTheme Amber(bool flatFill = false) => FromAccent(Color.FromRgb(0xE8, 0x9B, 0x2A), flatFill);

  /// <summary>
  /// Sky-blue accent.
  /// </summary>
  public static GraphTheme Sky(bool flatFill = false) => FromAccent(Color.FromRgb(0x3E, 0x9B, 0xE8), flatFill);

  /// <summary>
  /// Purple accent.
  /// </summary>
  public static GraphTheme Purple(bool flatFill = false) => FromAccent(Color.FromRgb(0x9B, 0x5A, 0xE8), flatFill);

  /// <summary>
  /// Builds a theme from a single accent color: the line and fill both derive from it, over a
  /// shared dark background/grid/border.
  ///
  /// The fill differs by <paramref name="flatFill"/>, and this is the important part, not just a
  /// detail: with <paramref name="flatFill"/> false (the default) the fill is a vertical gradient
  /// (solid-ish at the line, fading to transparent at the baseline) because
  /// <c>FilledLineRenderer</c> draws one continuous shape, so the gradient reads as a single smooth
  /// glow. With <paramref name="flatFill"/> true the fill is a flat SolidColorBrush instead — for a
  /// graph that draws many separate shapes, WPF's default relative gradient mapping restarts within
  /// each one it's used on, so the same "glow" gradient would instead repeat per shape. A flat fill
  /// is what actually reads correctly once the data is drawn as discrete pieces.
  /// </summary>
  public static GraphTheme FromAccent(Color accent, bool flatFill = false) {
    return new GraphTheme {
      GraphBackground = DefaultBackground,
      GridBrush = DefaultGrid,
      BorderBrush = DefaultBorder,
      LineBrush = Freeze(new SolidColorBrush(accent)),
      LineThickness = 1.5,
      // Only the continuous Line trace gets the vertical glow gradient; a flat fill is requested
      // for graphs that draw many separate shapes, where a gradient would restart in each.
      FillBrush = flatFill ? Freeze(new SolidColorBrush(accent)) : CreateVerticalGlow(accent)
    };
  }

  /// <summary>
  /// Creates a vertical gradient brush that fades from a solid accent color at the top to a more 
  /// transparent version of the same color at the bottom. This is used to create a "glow" effect 
  /// for the fill area under the graph line.
  /// </summary>
  /// <param name="accent">the accent color to use for the gradient</param>
  /// <returns>the vertical glow gradient brush</returns>
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

  /// <summary>
  /// Freezes the given brush if it can be frozen, making it immutable and improving performance.
  /// </summary>
  /// <param name="brush">the brush to freeze</param>
  /// <returns>the frozen brush</returns>
  private static Brush Freeze(Brush brush) {
    if (brush.CanFreeze) brush.Freeze();
    return brush;
  }
}
