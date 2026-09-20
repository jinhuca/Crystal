using Crystal.Controls.PerformanceGraphs.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Renders;

/// <summary>
/// Renders the border of a performance graph, drawing a rectangle around the specified bounds 
/// using the border pen defined in the graph style.
/// </summary>
internal sealed class BorderRenderer {
  /// <summary>
  /// Draws the border of the performance graph within the specified bounds using the provided drawing context and graph style.
  /// </summary>
  /// <param name="dc">The drawing context.</param>
  /// <param name="bounds">The bounds within which to draw the border.</param>
  /// <param name="style">The graph style containing the border pen.</param>
  public void Draw(DrawingContext dc, Rect bounds, GraphStyle style) {
    if (style.BorderThickness > 0 && style.BorderPen != null) {
      // First argument to DrawRectangle is the *fill* brush — pass null so this only
      // strokes the outline and doesn't paint over the grid/data drawn underneath it.
      dc.DrawRectangle(null, style.BorderPen, bounds);
    }
  }
}
