using Crystal.Controls.MeterGauges.Styles;
using System;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.MeterGauges.Renders;

/// <summary>
/// Draws the gauge's scale as a fan of short radial tick marks swept along an arc. Ticks up to
/// the current value are painted with the style's active brush, the remainder with the inactive
/// brush, giving the "filled meter" look from the reference image.
/// <para>
/// Angles are in degrees, measured clockwise in screen coordinates (y grows downward): 0° points
/// right (east), 90° down (south), 180° left (west), 270° up (north). The default sweep starts at
/// 135° (lower-left) and runs 270° clockwise up and over to 45° (lower-right), leaving the bottom
/// open — the classic dashboard gauge.
/// </para>
/// </summary>
internal sealed class TickArcRenderer {
  // Reused every frame by re-opening — one geometry batches every lit tick into a single
  // DrawGeometry call, another every unlit tick, so a full gauge is two draw calls regardless
  // of tick count instead of one per tick.
  private readonly StreamGeometry _activeGeometry = new();
  private readonly StreamGeometry _inactiveGeometry = new();

  public void Draw(
      DrawingContext dc,
      Rect bounds,
      GaugeStyle style,
      double fraction,
      int tickCount,
      double startAngle,
      double sweepAngle,
      double tickThickness,
      double innerRadiusRatio,
      double outerRadiusRatio) {
    if (tickCount <= 0) return;
    if (bounds.Width <= 0 || bounds.Height <= 0) return;

    fraction = fraction < 0 ? 0 : (fraction > 1 ? 1 : fraction);

    var center = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
    double radius = Math.Min(bounds.Width, bounds.Height) / 2;
    double innerR = radius * innerRadiusRatio;
    double outerR = radius * outerRadiusRatio;
    double halfWidth = tickThickness / 2;

    // A tick is "lit" when its position along the scale is at or below the current fraction.
    // The boundary tick index is inclusive so a full-scale value lights every tick.
    int litCount = (int)Math.Round(fraction * (tickCount - 1)) + 1;

    using (StreamGeometryContext activeCtx = _activeGeometry.Open())
    using (StreamGeometryContext inactiveCtx = _inactiveGeometry.Open()) {
      for (int i = 0; i < tickCount; i++) {
        double t = tickCount == 1 ? 0 : (double)i / (tickCount - 1);
        double angle = (startAngle + t * sweepAngle) * Math.PI / 180.0;

        double cos = Math.Cos(angle);
        double sin = Math.Sin(angle);
        // Unit vector perpendicular to the radial direction, giving the tick its width.
        double perpX = -sin * halfWidth;
        double perpY = cos * halfWidth;

        double ix = center.X + innerR * cos;
        double iy = center.Y + innerR * sin;
        double ox = center.X + outerR * cos;
        double oy = center.Y + outerR * sin;

        StreamGeometryContext ctx = i < litCount ? activeCtx : inactiveCtx;
        AddQuad(ctx,
            new Point(ix + perpX, iy + perpY),
            new Point(ox + perpX, oy + perpY),
            new Point(ox - perpX, oy - perpY),
            new Point(ix - perpX, iy - perpY));
      }
    }

    dc.DrawGeometry(style.InactiveBrush, null, _inactiveGeometry);
    dc.DrawGeometry(style.ActiveBrush, null, _activeGeometry);
  }

  private static void AddQuad(StreamGeometryContext ctx, Point a, Point b, Point c, Point d) {
    ctx.BeginFigure(a, isFilled: true, isClosed: true);
    ctx.LineTo(b, isStroked: false, isSmoothJoin: false);
    ctx.LineTo(c, isStroked: false, isSmoothJoin: false);
    ctx.LineTo(d, isStroked: false, isSmoothJoin: false);
  }
}
