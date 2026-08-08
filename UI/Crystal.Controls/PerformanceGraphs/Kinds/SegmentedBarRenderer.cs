using Crystal.Controls.PerformanceGraphs.Buffers;
using Crystal.Controls.PerformanceGraphs.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Kinds;

internal sealed class SegmentedBarRenderer {
  // Fraction of each column slot's width the bar itself occupies (same spacing as BarRenderer,
  // so switching between Bar and SegmentedBar doesn't change how wide each sample's column is).
  private const double BarWidthRatio = 0.7;

  // Fraction of each grid row's height a fully-lit segment occupies; the remainder is split
  // evenly as a gap above and below it, giving the classic stacked "LED meter" look. Segments
  // are one per grid row so their gaps line up with the horizontal grid lines.
  private const double SegmentHeightRatio = 0.8;

  // Created once, reused every frame by re-opening it — same reasoning as
  // FilledLineRenderer/BarRenderer. A single sample can contribute up to `rows` separate
  // segment figures, so without batching this could be hundreds of DrawRectangle calls per
  // frame (samples x rows); with it, the whole frame is one geometry and one DrawGeometry call.
  private readonly StreamGeometry _segmentsGeometry = new();

  public void Draw(
      DrawingContext dc,
      Rect bounds,
      GraphStyle style,
      CircularBuffer<double> values,
      int capacity,
      double minValue,
      double maxValue,
      int rows,
      bool flip = false) {
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
    double segmentHeight = rowHeight * SegmentHeightRatio;
    double segmentPadding = (rowHeight - segmentHeight) / 2;

    using (StreamGeometryContext ctx = _segmentsGeometry.Open()) {
      for (int i = 0; i < count; i++) {
        // Same right-aligned column layout as BarRenderer/FilledLineRenderer, so a column's
        // samples line up whichever GraphKind is active.
        double slotRight = bounds.Right - (count - 1 - i) * slotWidth;
        double left = slotRight - slotWidth + barInset;

        double t = (values[i] - minValue) / range;
        t = t < 0 ? 0 : (t > 1 ? 1 : t);

        // Total height a continuous bar would reach, expressed in whole rows plus a remainder.
        double fillHeight = t * bounds.Height;
        int fullSegments = (int)(fillHeight / rowHeight);
        if (fullSegments > rows) fullSegments = rows;

        double partialFraction = 0;
        if (fullSegments < rows) {
          partialFraction = (fillHeight - fullSegments * rowHeight) / rowHeight;
          partialFraction = partialFraction < 0 ? 0 : (partialFraction > 1 ? 1 : partialFraction);
        }

        if (flip) {
          // Mirrored (180°) variant: segments stack from the top row downward, and the partial
          // segment grows downward from the top anchor — the whole meter reads as hanging from
          // the top edge instead of rising from the bottom.
          for (int seg = 0; seg < fullSegments; seg++) {
            double rowTop = bounds.Top + seg * rowHeight;
            double segTop = rowTop + segmentPadding;
            AddRectangleFigure(ctx, left, segTop, barWidth, segmentHeight);
          }

          if (partialFraction > 0) {
            double rowTop = bounds.Top + fullSegments * rowHeight;
            double segTop = rowTop + segmentPadding;
            AddRectangleFigure(ctx, left, segTop, barWidth, partialFraction * segmentHeight);
          }
        } else {
          // Fully-lit segments, stacked from the bottom row upward.
          for (int seg = 0; seg < fullSegments; seg++) {
            double rowBottom = bounds.Bottom - seg * rowHeight;
            double segBottom = rowBottom - segmentPadding;
            AddRectangleFigure(ctx, left, segBottom - segmentHeight, barWidth, segmentHeight);
          }

          // The partially-lit segment just above the fully-lit ones (if any) grows upward from
          // the same bottom anchor a full segment in that row would use, so it reads as "this
          // row is X% lit" rather than jumping straight from empty to full.
          if (partialFraction > 0) {
            double rowBottom = bounds.Bottom - fullSegments * rowHeight;
            double segBottom = rowBottom - segmentPadding;
            double partialHeight = partialFraction * segmentHeight;
            AddRectangleFigure(ctx, left, segBottom - partialHeight, barWidth, partialHeight);
          }
        }
      }
    }

    dc.DrawGeometry(style.FillBrush, style.LinePen, _segmentsGeometry);
  }

  // Traces one closed, filled+strokeable rectangle figure directly into an already-open
  // context — no Rect/Point[] intermediate, no separate geometry per segment.
  private static void AddRectangleFigure(StreamGeometryContext ctx, double left, double top, double width, double height) {
    double right = left + width;
    double bottom = top + height;

    ctx.BeginFigure(new Point(left, top), isFilled: true, isClosed: true);
    ctx.LineTo(new Point(right, top), isStroked: true, isSmoothJoin: false);
    ctx.LineTo(new Point(right, bottom), isStroked: true, isSmoothJoin: false);
    ctx.LineTo(new Point(left, bottom), isStroked: true, isSmoothJoin: false);
  }
}
