using Crystal.Controls.PerformanceGraphs.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Renders;

// Draws horizontal reference lines at fixed data values — used for session extremes (the lowest/
// highest sample seen) so a past dip or spike stays visible after the live line has recovered.
internal sealed class MarkerRenderer {
  public void Draw(DrawingContext dc, Rect bounds, GraphStyle style, double value, double minValue, double maxValue) {
    if (style.MarkerPen is null) return;
    if (double.IsNaN(value)) return;
    if (bounds.Width <= 0 || bounds.Height <= 0) return;

    double range = maxValue - minValue;
    if (range <= 0) return;

    double t = (value - minValue) / range;
    t = t < 0 ? 0 : (t > 1 ? 1 : t);
    double y = bounds.Bottom - t * bounds.Height;
    dc.DrawLine(style.MarkerPen, new Point(bounds.Left, y), new Point(bounds.Right, y));
  }
}
