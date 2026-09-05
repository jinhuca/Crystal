using Crystal.Controls.PerformanceGraphs.Buffers;
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
      CircularBuffer<double> values,
      int capacity,
      double minValue,
      double maxValue,
      Pen? linePen,
      Brush? fillBrush,
      Pen[]? bandPens = null,
      Brush[]? bandFills = null,
      double cellPitch = 0) {
    int count = values.Count;
    if (count < 2) return;
    if (bounds.Width <= 0 || bounds.Height <= 0) return;

    double range = maxValue - minValue;
    if (range <= 0) return;

    // Banded mode colors the line and its fill by value: horizontal value-bands are painted under
    // the curve, and the line itself is drawn once per band clipped to that band's vertical strip,
    // so a segment reads green where it sits low on the scale and red where it spikes. Requires the
    // fill geometry regardless of fillBrush, since the bands are what paint the area.
    bool banded = bandPens != null && bandFills != null;

    // Fixed-pitch mode (cellPitch > 0) mirrors PerformanceGraphLite: samples sit at a fixed pixel
    // pitch and only the most recent that fit the width are drawn (older ones scroll off the left).
    // A Line and a Dot graph sharing one pitch then show the same time window, so toggling between
    // them keeps the visible history identical instead of re-scaling it. Otherwise every sample owns
    // one horizontal slot spread across Capacity, just like BarRenderer.
    int start = 0;
    double slotWidth;
    if (cellPitch > 0) {
      int cols = Math.Max(1, (int)Math.Round(bounds.Width / cellPitch));
      slotWidth = bounds.Width / cols;
      if (count > cols) start = count - cols;
    } else {
      int effectiveCapacity = capacity > count ? capacity : count;
      slotWidth = bounds.Width / effectiveCapacity;
    }
    if (count - start < 2) return;

    // Newest sample (last, index count-1) is in the centre of the rightmost occupied slot; each
    // older drawn sample steps one slot to the left. The slot offset counts from the right, so the
    // same x formula holds whether or not older samples were skipped.
    double firstX = bounds.Right - (count - start - 0.5) * slotWidth;
    double firstY = ComputeY(values[start], minValue, range, bounds);
    var firstPoint = new Point(firstX, firstY);
    var capStart = new Point(firstPoint.X - slotWidth / 2, firstPoint.Y);

    // fillBrush is documented as commonly null (an overlaid series drawn as a plain line, so it
    // doesn't occlude the one underneath) - skip opening/building _fillGeometry entirely in that
    // case rather than tracing every sample into a geometry that's never drawn. linePen isn't
    // given the same treatment: both call sites (the primary series' LineBrush-derived pen and
    // AddSeries's required lineBrush parameter) always produce a non-null Pen in practice, so
    // there's no real case where that work would actually go to waste.
    bool needsFill = fillBrush != null || banded;
    StreamGeometryContext? fillCtx = needsFill ? _fillGeometry.Open() : null;
    try {
      using (StreamGeometryContext lineCtx = _lineGeometry.Open()) {
        // Leading half-slot cap: fills the whole occupied first slot while the measured
        // vertex stays centred within its cell.
        if (fillCtx != null) {
          fillCtx.BeginFigure(new Point(capStart.X, bounds.Bottom), isFilled: true, isClosed: true);
          fillCtx.LineTo(capStart, isStroked: false, isSmoothJoin: false);
          fillCtx.LineTo(firstPoint, isStroked: false, isSmoothJoin: false);
        }

        lineCtx.BeginFigure(capStart, isFilled: false, isClosed: false);
        lineCtx.LineTo(firstPoint, isStroked: true, isSmoothJoin: true);

        Point previousPoint = firstPoint;

        for (int i = start + 1; i < count; i++) {
          double x = bounds.Right - (count - i - 0.5) * slotWidth;
          double y = ComputeY(values[i], minValue, range, bounds);
          var point = new Point(x, y);

          fillCtx?.LineTo(point, isStroked: false, isSmoothJoin: false);
          lineCtx.LineTo(point, isStroked: true, isSmoothJoin: true);

          previousPoint = point;
        }

        // Trailing half-slot cap, mirroring the leading one.
        var capEnd = new Point(previousPoint.X + slotWidth / 2, previousPoint.Y);
        if (fillCtx != null) {
          fillCtx.LineTo(capEnd, isStroked: false, isSmoothJoin: false);
          fillCtx.LineTo(new Point(capEnd.X, bounds.Bottom), isStroked: false, isSmoothJoin: false);
        }
        lineCtx.LineTo(capEnd, isStroked: true, isSmoothJoin: true);
      }
    } finally {
      fillCtx?.Close();
    }

    if (banded) {
      PaintBanded(dc, bounds, minValue, range, bandPens!, bandFills!);
      return;
    }

    if (fillBrush != null) {
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
      dc.DrawRectangle(fillBrush, null, bounds);
      dc.Pop();
    }
    if (linePen != null) {
      dc.DrawGeometry(null, linePen, _lineGeometry);
    }
  }

  // Value-banded paint: the plotted range is split into bandFills.Length equal-height horizontal
  // strips (band 0 lowest). The fill under the curve is painted band-by-band by clipping to the
  // area geometry and drawing each band's strip in its translucent color, so it stacks green→red
  // from the baseline up to the curve. The line is then drawn once per band, each pass clipped to
  // that band's strip and stroked in the band's solid color, so the line changes color as it rises
  // and falls through the scale. Both reuse the geometries already built above.
  private void PaintBanded(DrawingContext dc, Rect bounds, double minValue, double range, Pen[] bandPens, Brush[] bandFills) {
    int bands = bandFills.Length;

    dc.PushClip(_fillGeometry);
    for (int b = 0; b < bands; b++) {
      Rect strip = BandStrip(b, bands, bounds, minValue, range);
      if (strip.Height > 0) dc.DrawRectangle(bandFills[b], null, strip);
    }
    dc.Pop();

    for (int b = 0; b < bands; b++) {
      if (bandPens[b] == null) continue;
      Rect strip = BandStrip(b, bands, bounds, minValue, range);
      if (strip.Height <= 0) continue;
      var clip = new RectangleGeometry(strip);
      clip.Freeze();
      dc.PushClip(clip);
      dc.DrawGeometry(null, bandPens[b], _lineGeometry);
      dc.Pop();
    }
  }

  // The pixel rectangle spanning the full plot width and the vertical extent of value-band `band`
  // (of `bands` equal-value bands across [minValue, minValue+range]).
  private static Rect BandStrip(int band, int bands, Rect bounds, double minValue, double range) {
    double lowValue = minValue + band / (double)bands * range;
    double highValue = minValue + (band + 1) / (double)bands * range;
    double yTop = ComputeY(highValue, minValue, range, bounds);
    double yBottom = ComputeY(lowValue, minValue, range, bounds);
    return new Rect(bounds.Left, yTop, bounds.Width, yBottom - yTop);
  }

  private static double ComputeY(double value, double minValue, double range, Rect bounds) {
    double t = (value - minValue) / range;
    t = t < 0 ? 0 : (t > 1 ? 1 : t);
    return bounds.Bottom - t * bounds.Height;
  }
}
