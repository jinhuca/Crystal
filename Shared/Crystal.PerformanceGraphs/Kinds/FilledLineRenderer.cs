using Crystal.PerformanceGraphs.Buffers;
using Crystal.PerformanceGraphs.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.PerformanceGraphs.Kinds;

internal sealed class FilledLineRenderer {
  // Created once, reused every frame by re-opening them — never frozen. StreamGeometry can
  // be re-Open()'d any number of times as long as it isn't frozen, and each Open()/dispose
  // cycle replaces its prior content, so this is the whole allocation for the lifetime of
  // the renderer rather than one `new StreamGeometry()` per frame. Freezing would have been
  // pointless here anyway: the original code froze a geometry it was about to draw once and
  // discard next frame — freezing buys nothing for something with a one-frame lifetime, and
  // it would also prevent the reuse this depends on.
  private readonly StreamGeometry _fillGeometry = new();
  private readonly StreamGeometry _lineGeometry = new();
  private LinearGradientBrush? _absoluteGradientBrush;
  private LinearGradientBrush? _gradientSource;

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
    // With matching Capacity and GridColumns, this makes each vertex coincide with the centre
    // of the corresponding grid cell/bar slot instead of drifting almost a whole cell by the
    // penultimate sample.
    double slotWidth = bounds.Width / effectiveCapacity;

    // Newest sample (last, index count-1) is in the centre of the rightmost occupied slot;
    // each older sample steps one slot to the left. This is the same horizontal coordinate as
    // the centre of the matching bar.
    double firstX = bounds.Right - (count - 0.5) * slotWidth;
    double firstY = ComputeY(values[0], minValue, range, bounds);
    var firstPoint = new Point(firstX, firstY);
    double lastX = firstX;
    double lastY = firstY;

    // Stream every sample straight into both geometry contexts in one pass — no
    // intermediate Point[] array, ever, at any buffer size.
    using (StreamGeometryContext fillCtx = _fillGeometry.Open())
    using (StreamGeometryContext lineCtx = _lineGeometry.Open()) {
      // Fill the whole occupied first/last slot, while the measured vertices remain centred
      // within their cells. Thus a full buffer reaches both plot edges without changing the
      // sample-to-grid alignment.
      double firstSlotLeft = firstPoint.X - slotWidth / 2;
      fillCtx.BeginFigure(new Point(firstSlotLeft, bounds.Bottom), isFilled: true, isClosed: true);
      fillCtx.LineTo(new Point(firstSlotLeft, firstPoint.Y), isStroked: false, isSmoothJoin: false);
      fillCtx.LineTo(firstPoint, isStroked: false, isSmoothJoin: false);

      // Stroke the same horizontal extensions as the fill so the first and last occupied
      // half-slots have a solid top edge instead of appearing to stop at the sample centres.
      lineCtx.BeginFigure(new Point(firstSlotLeft, firstPoint.Y), isFilled: false, isClosed: false);
      lineCtx.LineTo(firstPoint, isStroked: true, isSmoothJoin: true);

      for (int i = 1; i < count; i++) {
        double x = bounds.Right - (count - i - 0.5) * slotWidth;
        double y = ComputeY(values[i], minValue, range, bounds);
        var point = new Point(x, y);

        fillCtx.LineTo(point, isStroked: false, isSmoothJoin: false);
        lineCtx.LineTo(point, isStroked: true, isSmoothJoin: true);

        lastX = x;
        lastY = y;
      }

      double lastSlotRight = lastX + slotWidth / 2;
      fillCtx.LineTo(new Point(lastSlotRight, lastY), isStroked: false, isSmoothJoin: false);
      fillCtx.LineTo(new Point(lastSlotRight, bounds.Bottom), isStroked: false, isSmoothJoin: false);
      lineCtx.LineTo(new Point(lastSlotRight, lastY), isStroked: true, isSmoothJoin: true);
    }

    if (style.FillBrush != null) {
      dc.DrawGeometry(ResolveFillBrush(style.FillBrush, bounds), null, _fillGeometry);
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

  private Brush ResolveFillBrush(Brush brush, Rect bounds) {
    if (brush is not LinearGradientBrush source) return brush;

    // Theme brushes are frozen/shared. Clone the source once, then place the clone in absolute
    // plot coordinates each frame so the glow remains one smooth, stable vertical gradient.
    if (!ReferenceEquals(source, _gradientSource)) {
      _gradientSource = source;
      _absoluteGradientBrush = source.Clone();
      _absoluteGradientBrush.MappingMode = BrushMappingMode.Absolute;
    }

    _absoluteGradientBrush!.StartPoint = new Point(bounds.Left, bounds.Top);
    _absoluteGradientBrush.EndPoint = new Point(bounds.Left, bounds.Bottom);
    return _absoluteGradientBrush;
  }
}
