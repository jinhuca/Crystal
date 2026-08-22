using Crystal.Controls.PerformanceGraphs.Buffers;
using Crystal.Controls.PerformanceGraphs.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Kinds;

internal sealed class DotRenderer {
  // Fraction of each column slot's width the dot field occupies (same spacing as
  // Bar/SegmentedBar, so switching kinds doesn't change where a sample's column sits).
  private const double BarWidthRatio = 0.7;

  // Side length of each dot as a fraction of the row pitch; the remainder is the gap that gives
  // the sparse "dot-matrix" look (btop-style). A square lattice: dots are pitched by row height
  // both vertically and horizontally.
  private const double DotSizeRatio = 0.55;

  // Same batching rationale as the other discrete renderers: a sample can contribute up to
  // rows x dots-per-column separate dot figures, so without batching this could be hundreds of
  // DrawRectangle calls per frame; with it the whole frame is one geometry and one DrawGeometry.
  private readonly StreamGeometry _dotsGeometry = new();

  // Dots are many separate squares, so any gradient fill is flattened to a flat colour and the
  // stroke shares it — a dot reads as one solid speck rather than a two-tone fill + outline.
  private readonly SolidFillCache _solidFill = new();

  public void Draw(
      DrawingContext dc,
      Rect bounds,
      GraphStyle style,
      CircularBuffer<double> values,
      int capacity,
      double minValue,
      double maxValue,
      int rows) {
    int count = values.Count;
    if (count == 0) return;
    if (bounds.Width <= 0 || bounds.Height <= 0) return;
    if (rows <= 0) return;

    double range = maxValue - minValue;
    if (range <= 0) return;

    int effectiveCapacity = capacity > count ? capacity : count;
    double slotWidth = bounds.Width / effectiveCapacity;
    double barWidth = slotWidth * BarWidthRatio;
    double barInset = (slotWidth - barWidth) / 2;

    double rowHeight = bounds.Height / rows;
    // One dot per column, centred in the slot. Sizing it off the row pitch keeps every graph's
    // dots the same size regardless of tile width; a column is never split into 2+ dots across
    // (which is what made wide/short tiles look "double-dotted" while narrow ones stayed single).
    double dotSize = rowHeight * DotSizeRatio;
    if (dotSize > barWidth) dotSize = barWidth;

    using (StreamGeometryContext ctx = _dotsGeometry.Open()) {
      for (int i = 0; i < count; i++) {
        // Same right-aligned column layout as the other renderers, so a column's dots line up
        // whichever GraphKind is active.
        double slotRight = bounds.Right - (count - 1 - i) * slotWidth;
        double left = slotRight - slotWidth + barInset;
        double cx = left + barWidth / 2;

        double t = (values[i] - minValue) / range;
        t = t < 0 ? 0 : (t > 1 ? 1 : t);

        // Number of dot rows lit from the bottom up. Round so a value that reaches most of a row
        // lights it, matching the meter-like feel of the segmented bar.
        double fillHeight = t * bounds.Height;
        int litRows = (int)(fillHeight / rowHeight + 0.5);
        if (litRows > rows) litRows = rows;
        // A non-zero value always lights at least the bottom row so low readings stay visible.
        if (litRows == 0 && t > 0) litRows = 1;

        for (int r = 0; r < litRows; r++) {
          double cy = bounds.Bottom - (r + 0.5) * rowHeight;
          AddRectangleFigure(ctx, cx - dotSize / 2, cy - dotSize / 2, dotSize, dotSize);
        }
      }
    }

    (Brush fill, Pen pen) = _solidFill.Resolve(style.FillBrush, style.LinePen.Thickness);
    dc.DrawGeometry(fill, pen, _dotsGeometry);
  }

  // Traces one closed, filled+strokeable square figure directly into an already-open context.
  private static void AddRectangleFigure(StreamGeometryContext ctx, double left, double top, double width, double height) {
    double right = left + width;
    double bottom = top + height;

    ctx.BeginFigure(new Point(left, top), isFilled: true, isClosed: true);
    ctx.LineTo(new Point(right, top), isStroked: true, isSmoothJoin: false);
    ctx.LineTo(new Point(right, bottom), isStroked: true, isSmoothJoin: false);
    ctx.LineTo(new Point(left, bottom), isStroked: true, isSmoothJoin: false);
  }
}
