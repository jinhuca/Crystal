using Crystal.Controls.PerformanceGraphs.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Renders;

internal sealed class BackgroundRenderer {
  public void Draw(DrawingContext dc, Rect bounds, GraphStyle style) {
    if (style.BackgroundBrush != null) {
      dc.DrawRectangle(style.BackgroundBrush, null, bounds);
    }
  }
}
