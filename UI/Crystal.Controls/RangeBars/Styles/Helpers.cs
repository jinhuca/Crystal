using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.RangeBars.Styles;

/// <summary>
/// Contains helper methods for rendering <see cref="RangeBar"/>s and their components.
/// </summary>
internal static class Helpers {
  /// <summary>
  /// Creates a <see cref="Pen"/> with the specified <paramref name="brush"/> and <paramref name="thickness"/>,
  /// freezing it for performance.
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
  /// Insets <paramref name="bounds"/> by half of <paramref name="thickness"/> on each edge — the
  /// interior a border of that thickness leaves once its edge-straddling stroke is accounted for.
  /// The deflated size is clamped to zero because <see cref="Rect"/> throws on a negative
  /// width/height, which a control smaller than the border thickness would otherwise produce.
  /// The fill interior and the border stroke share this rectangle so they stay aligned.
  /// </summary>
  /// <param name="bounds">The bounds to deflate.</param>
  /// <param name="thickness">The thickness by which to deflate the bounds.</param>
  /// <returns>The deflated bounds.</returns>
  public static Rect Deflate(Rect bounds, double thickness) {
    double inset = thickness / 2;
    return new Rect(
      bounds.X + inset,
      bounds.Y + inset,
      Math.Max(0, bounds.Width - thickness),
      Math.Max(0, bounds.Height - thickness));
  }
}
