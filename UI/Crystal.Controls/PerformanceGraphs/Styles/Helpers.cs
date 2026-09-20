using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Styles;
internal static class Helpers {
  /// <summary>
  /// The representative color of a brush: its color for a <see cref="SolidColorBrush"/>, or the
  /// most-opaque gradient stop for a <see cref="GradientBrush"/> (matching how the banded Line
  /// picks a solid color from a gradient). Anything else maps to transparent.
  /// </summary>
  /// <param name="brush">The brush to inspect.</param>
  /// <returns>The representative color.</returns>
  public static Color ToColor(Brush brush) {
    if (brush is SolidColorBrush solid) return solid.Color;
    if (brush is GradientBrush gradient && gradient.GradientStops.Count > 0) {
      GradientStop pick = gradient.GradientStops[0];
      foreach (GradientStop stop in gradient.GradientStops)
        if (stop.Color.A > pick.Color.A) pick = stop;
      return pick.Color;
    }
    return Colors.Transparent;
  }

  /// <summary>
  /// A frozen top-to-bottom glow brush in the given accent color, translucent at the top and fainter
  /// at the baseline — the faint area fill derived under a data line when no explicit fill is set.
  /// </summary>
  /// <param name="accent">The accent color.</param>
  /// <param name="topAlpha">Alpha at the top of the fill.</param>
  /// <param name="bottomAlpha">Alpha at the baseline.</param>
  /// <returns>The frozen glow brush.</returns>
  public static Brush CreateVerticalGlow(Color accent, byte topAlpha = 0x80, byte bottomAlpha = 0x20) {
    var brush = new LinearGradientBrush {
      StartPoint = new Point(0, 0),
      EndPoint = new Point(0, 1)
    };
    brush.GradientStops.Add(new GradientStop(Color.FromArgb(topAlpha, accent.R, accent.G, accent.B), 0));
    brush.GradientStops.Add(new GradientStop(Color.FromArgb(bottomAlpha, accent.R, accent.G, accent.B), 1));
    brush.Freeze();
    return brush;
  }

  /// <summary>
  /// Creates a frozen pen with the specified brush and thickness. 
  /// Freezing the pen improves performance by making it immutable and thread-safe.
  /// </summary>
  /// <param name="brush">The brush to use for the pen.</param>
  /// <param name="thickness">The thickness of the pen.</param>
  /// <returns>The frozen pen.</returns>
  public static Pen CreateFrozenPen(Brush brush, double thickness) {
    var pen = new Pen(brush, thickness);
    pen.Freeze();
    return pen;
  }

  /// <summary>
  /// A thin dashed pen for reference/marker lines, distinct from the solid data line.
  /// </summary>
  /// <param name="brush">The brush to use for the pen.</param>
  /// <param name="thickness">The thickness of the pen.</param>
  /// <returns>The frozen pen.</returns>
  public static Pen CreateDashedPen(Brush brush, double thickness) {
    var pen = new Pen(brush, thickness) { DashStyle = new DashStyle([4, 3], 0) };
    pen.Freeze();
    return pen;
  }
}
