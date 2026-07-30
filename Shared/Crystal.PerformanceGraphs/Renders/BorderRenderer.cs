using Crystal.PerformanceGraphs.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.PerformanceGraphs.Renders;

internal sealed class BorderRenderer {
  public void Draw(DrawingContext dc, Rect bounds, GraphStyle style) {
    if (style.BorderThickness > 0 && style.BorderPen != null) {
      // First argument to DrawRectangle is the *fill* brush — pass null so this only
      // strokes the outline and doesn't paint over the grid/data drawn underneath it.
      dc.DrawRectangle(null, style.BorderPen, bounds);
    }
  }
}
