using System;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.RangeBars.Styles;

internal static class Helpers {
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
  public static Rect Deflate(Rect bounds, double thickness) {
    double inset = thickness / 2;
    return new Rect(
        bounds.X + inset,
        bounds.Y + inset,
        Math.Max(0, bounds.Width - thickness),
        Math.Max(0, bounds.Height - thickness));
  }
}
