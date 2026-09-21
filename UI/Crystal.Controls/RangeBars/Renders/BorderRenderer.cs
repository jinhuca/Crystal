using Crystal.Controls.RangeBars.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.RangeBars.Renders;

/// <summary>
/// Renders the border of a <see cref="RangeBar"/> using the specified <see cref="RangeBarStyle"/>.
/// </summary>
internal sealed class BorderRenderer {
  /// <summary>
  /// Draws the border of a <see cref="RangeBar"/> using the specified <see cref="RangeBarStyle"/>.
  /// </summary>
  /// <param name="dc">The drawing context.</param>
  /// <param name="bounds">The bounds of the border.</param>
  /// <param name="style">The style to use for rendering.</param>
  public void Draw(DrawingContext dc, Rect bounds, RangeBarStyle style) {
    if (style.BorderThickness <= 0 || style.BorderPen == null) return;

    Rect rect = Helpers.Deflate(bounds, style.BorderThickness);
    if (rect.Width <= 0 || rect.Height <= 0) return;

    // First argument is the *fill* brush — pass null so this only strokes the outline.
    dc.DrawRectangle(null, style.BorderPen, rect);
  }
}
