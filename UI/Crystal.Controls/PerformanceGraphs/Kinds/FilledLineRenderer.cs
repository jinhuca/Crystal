using Crystal.Controls.PerformanceGraphs.Buffers;
using Crystal.Controls.PerformanceGraphs.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Kinds;

internal sealed class FilledLineRenderer {
  // Reused every frame by re-opening them — never frozen. See PerformanceGraph's OnRender
  // ordering comment for why: StreamGeometry can be re-Open()'d indefinitely as long as it
  // isn't frozen, so this is the whole allocation for this renderer's lifetime.
  private readonly StreamGeometry _fillGeometry = new();
  private readonly StreamGeometry _lineGeometry = new();

  public void Draw(
      DrawingContext dc,
      Rect bounds,
      GraphStyle style,
      CircularBuffer<double> values,
      int capacity,
      double minValue,
      double maxValue) {
    int count = values.Count;
    if (count < 2) return;
    if (bounds.Width <= 0 || bounds.Height <= 0) return;

    double range = maxValue - minValue;
    if (range <= 0) return;

    int effectiveCapacity = capacity > count ? capacity : count;
    // Every sample owns one horizontal slot, just like BarRenderer. Keep each line vertex at
    // that slot's centre, rather than spreading vertices across (capacity - 1) wider gaps.
    double slotWidth = bounds.Width / effectiveCapacity;

    // Newest sample (last, index count-1) is in the centre of the rightmost occupied slot;
    // each older sample steps one slot to the left.
    double firstX = bounds.Right - (count - 0.5) * slotWidth;
    double firstY = ComputeY(values[0], minValue, range, bounds);
    var firstPoint = new Point(firstX, firstY);
    var capStart = new Point(firstPoint.X - slotWidth / 2, firstPoint.Y);

    using (StreamGeometryContext fillCtx = _fillGeometry.Open())
    using (StreamGeometryContext lineCtx = _lineGeometry.Open()) {
      // Leading half-slot cap: fills the whole occupied first slot while the measured
      // vertex stays centred within its cell.
      fillCtx.BeginFigure(new Point(capStart.X, bounds.Bottom), isFilled: true, isClosed: true);
      fillCtx.LineTo(capStart, isStroked: false, isSmoothJoin: false);
      fillCtx.LineTo(firstPoint, isStroked: false, isSmoothJoin: false);

      lineCtx.BeginFigure(capStart, isFilled: false, isClosed: false);
      lineCtx.LineTo(firstPoint, isStroked: true, isSmoothJoin: true);

      Point previousPoint = firstPoint;

      for (int i = 1; i < count; i++) {
        double x = bounds.Right - (count - i - 0.5) * slotWidth;
        double y = ComputeY(values[i], minValue, range, bounds);
        var point = new Point(x, y);

        fillCtx.LineTo(point, isStroked: false, isSmoothJoin: false);
        lineCtx.LineTo(point, isStroked: true, isSmoothJoin: true);

        previousPoint = point;
      }

      // Trailing half-slot cap, mirroring the leading one.
      var capEnd = new Point(previousPoint.X + slotWidth / 2, previousPoint.Y);
      fillCtx.LineTo(capEnd, isStroked: false, isSmoothJoin: false);
      fillCtx.LineTo(new Point(capEnd.X, bounds.Bottom), isStroked: false, isSmoothJoin: false);
      lineCtx.LineTo(capEnd, isStroked: true, isSmoothJoin: true);
    }

    if (style.FillBrush != null) {
      // Clip to the area under the line, then fill a rectangle spanning the WHOLE plot
      // area — not just DrawGeometry(fillBrush, null, _fillGeometry) directly. The reason:
      // a gradient brush's default RelativeToBoundingBox mapping scales to the bounding box
      // of whatever shape it's asked to paint. Painting _fillGeometry directly means that
      // "whatever shape" is the visible area under the line — so the gradient's full-color
      // end silently re-anchors itself to whichever sample is tallest CURRENTLY ON SCREEN,
      // and that reference shifts as data scrolls in and out of view. Painting a rectangle
      // that always spans the full plot bounds instead gives the brush a fixed, stable
      // bounding box (top of the grid, at MaxValue) every frame regardless of what data is
      // visible — then clipping restricts the actually-painted pixels to the same area as
      // before. One shape, one gradient reference, no seams between pieces.
      dc.PushClip(_fillGeometry);
      dc.DrawRectangle(style.FillBrush, null, bounds);
      dc.Pop();
    }
    if (style.LinePen != null) {
      dc.DrawGeometry(null, style.LinePen, _lineGeometry);
    }
  }

  private static double ComputeY(double value, double minValue, double range, Rect bounds) {
    double t = (value - minValue) / range;
    t = t < 0 ? 0 : (t > 1 ? 1 : t);
    return bounds.Bottom - t * bounds.Height;
  }
}
