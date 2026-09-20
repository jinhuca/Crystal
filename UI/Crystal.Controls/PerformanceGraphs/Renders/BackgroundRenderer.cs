using Crystal.Controls.PerformanceGraphs.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Renders;

/// <summary>
/// Renders the background of a performance graph, filling the specified bounds with the background 
/// brush defined in the graph style.
/// </summary>
internal sealed class BackgroundRenderer {
  /// <summary>
  /// Draws the background of the performance graph within the specified bounds using the provided drawing context and graph style.
  /// </summary>
  /// <param name="dc">The drawing context.</param>
  /// <param name="bounds">The bounds within which to draw the background.</param>
  /// <param name="style">The graph style containing the background brush.</param>
  public void Draw(DrawingContext dc, Rect bounds, GraphStyle style) {
    if (style.BackgroundBrush != null) {
      dc.DrawRectangle(style.BackgroundBrush, null, bounds);
    }
  }
}
