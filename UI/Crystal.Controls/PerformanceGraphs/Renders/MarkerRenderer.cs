using Crystal.Controls.PerformanceGraphs.Styles;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Renders;

// Draws horizontal reference lines at fixed data values — used for session extremes (the lowest/
// highest sample seen) so a past dip or spike stays visible after the live line has recovered. When
// a label is supplied, the value is also written in the corner the line hugs, so a marker reads as a
// number rather than just a position.
internal sealed class MarkerRenderer {
  private const double LabelFontSize = 9;
  private const double LabelPadding = 2;
  private static readonly Typeface LabelTypeface = new("Consolas");

  // topBiased: true for the high marker (label sits below the line, growing down into the plot),
  // false for the low marker (label sits above the line, growing up) — so neither label is clipped
  // at the plot edge and the two never overlap when the markers are far apart.
  public void Draw(DrawingContext dc, Rect bounds, GraphStyle style, double value, double minValue, double maxValue,
      string? label = null, bool topBiased = false, double pixelsPerDip = 1.0) {
    if (style.MarkerPen is null) return;
    if (double.IsNaN(value)) return;
    if (bounds.Width <= 0 || bounds.Height <= 0) return;

    double range = maxValue - minValue;
    if (range <= 0) return;

    double t = (value - minValue) / range;
    t = t < 0 ? 0 : (t > 1 ? 1 : t);
    double y = bounds.Bottom - t * bounds.Height;
    dc.DrawLine(style.MarkerPen, new Point(bounds.Left, y), new Point(bounds.Right, y));

    if (string.IsNullOrEmpty(label)) return;
    var text = new FormattedText(label, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
        LabelTypeface, LabelFontSize, style.MarkerPen.Brush, pixelsPerDip);
    // Right-align against the plot edge; drop below the line for the high marker, lift above it for
    // the low one, then clamp so a marker near an edge keeps its whole label inside the plot.
    double x = bounds.Right - text.Width - LabelPadding;
    double textY = topBiased ? y + LabelPadding : y - text.Height - LabelPadding;
    if (textY < bounds.Top) textY = bounds.Top;
    if (textY + text.Height > bounds.Bottom) textY = bounds.Bottom - text.Height;
    dc.DrawText(text, new Point(x, textY));
  }
}
