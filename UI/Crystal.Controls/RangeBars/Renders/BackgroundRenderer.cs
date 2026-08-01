using Crystal.Controls.RangeBars.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.RangeBars.Renders;

internal sealed class BackgroundRenderer {
  public void Draw(DrawingContext dc, Rect bounds, RangeBarStyle style) {
    if (style.BackgroundBrush != null) {
      dc.DrawRectangle(style.BackgroundBrush, null, bounds);
    }
  }
}
