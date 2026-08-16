using Crystal.Controls.PerformanceGraphs.Buffers;
using Crystal.Controls.PerformanceGraphs.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Kinds;

internal sealed class BarRenderer {
  // Fraction of each slot's width the bar itself occupies; the remainder is split evenly
  // as a gap on either side, giving the classic "spaced bars" look.
  private const double BarWidthRatio = 0.7;

  // Created once, reused every frame by re-opening it — same reasoning as
  // FilledLineRenderer. A StreamGeometry can hold any number of disjoint closed figures in
  // a single Open() session, so every bar in a frame becomes one figure in this one
  // geometry, and the whole frame's bars are drawn with a single DrawGeometry call instead
  // of one DrawRectangle call per bar.
  private readonly StreamGeometry _barsGeometry = new();

  // Bars are separate rectangles, so fill and outline are drawn as one uniform solid: any gradient
  // fill is flattened to a flat colour and the stroke shares that same brush, so a bar reads as one
  // solid block rather than a two-tone fill + accent outline. Cached so a static fill isn't rebuilt.
  private readonly SolidFillCache _solidFill = new();

  public void Draw(
      DrawingContext dc,
      Rect bounds,
      GraphStyle style,
      CircularBuffer<double> values,
      int capacity,
      double minValue,
      double maxValue) {
    int count = values.Count;
    if (count == 0) return;
    if (bounds.Width <= 0 || bounds.Height <= 0) return;

    double range = maxValue - minValue;
    if (range <= 0) return;

    int effectiveCapacity = capacity > count ? capacity : count;
    double slotWidth = bounds.Width / effectiveCapacity;
    double barWidth = slotWidth * BarWidthRatio;
    double barInset = (slotWidth - barWidth) / 2;

    using (StreamGeometryContext ctx = _barsGeometry.Open()) {
      for (int i = 0; i < count; i++) {
        // Newest sample (last in the list) is pinned to the right edge; each older sample
        // steps one slot to the left — the same layout FilledLineRenderer uses, so switching
        // GraphKind doesn't shift where samples line up against the grid.
        double slotRight = bounds.Right - (count - 1 - i) * slotWidth;
        double left = slotRight - slotWidth + barInset;

        double t = (values[i] - minValue) / range;
        t = t < 0 ? 0 : (t > 1 ? 1 : t);
        double barTop = bounds.Bottom - t * bounds.Height;

        AddRectangleFigure(ctx, left, barTop, barWidth, bounds.Bottom - barTop);
      }
    }

    (Brush fill, Pen pen) = _solidFill.Resolve(style.FillBrush, style.LinePen.Thickness);
    dc.DrawGeometry(fill, pen, _barsGeometry);
  }

  // Traces one closed, filled+strokeable rectangle figure directly into an already-open
  // context — no Rect/Point[] intermediate, no separate geometry per bar.
  private static void AddRectangleFigure(StreamGeometryContext ctx, double left, double top, double width, double height) {
    double right = left + width;
    double bottom = top + height;

    ctx.BeginFigure(new Point(left, top), isFilled: true, isClosed: true);
    ctx.LineTo(new Point(right, top), isStroked: true, isSmoothJoin: false);
    ctx.LineTo(new Point(right, bottom), isStroked: true, isSmoothJoin: false);
    ctx.LineTo(new Point(left, bottom), isStroked: true, isSmoothJoin: false);
  }
}
