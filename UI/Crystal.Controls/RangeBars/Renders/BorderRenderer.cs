using Crystal.Controls.RangeBars.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.RangeBars.Renders;

internal sealed class BorderRenderer {
  public void Draw(DrawingContext dc, Rect bounds, RangeBarStyle style) {
    if (style.BorderThickness <= 0 || style.BorderPen == null) return;

    Rect rect = Helpers.Deflate(bounds, style.BorderThickness);
    if (rect.Width <= 0 || rect.Height <= 0) return;

    // First argument is the *fill* brush — pass null so this only strokes the outline.
    dc.DrawRectangle(null, style.BorderPen, rect);
  }
}
