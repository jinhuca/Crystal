using Crystal.Controls.RangeBars.Styles;
using System;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.RangeBars.Renders;

/// <summary>
/// Draws the range bar's interior: the full-width track with the style's track brush, then the
/// filled portion — from the left edge up to <paramref name="fraction"/> of the width — with the
/// style's fill brush. The interior is inset by the border thickness so the fill sits inside the
/// border stroke rather than under it.
/// </summary>
internal sealed class FillRenderer {
  public void Draw(DrawingContext dc, Rect bounds, RangeBarStyle style, double fraction) {
    fraction = Math.Clamp(fraction, 0, 1);

    // The interior sits inside the border stroke, which straddles the edge.
    Rect interior = Helpers.Deflate(bounds, style.BorderThickness);
    if (interior.Width <= 0 || interior.Height <= 0) return;

    if (style.TrackBrush != null) {
      dc.DrawRectangle(style.TrackBrush, null, interior);
    }

    double fillWidth = interior.Width * fraction;
    if (fillWidth > 0 && style.FillBrush != null) {
      if (style.Segmented) {
        // LED blocks left→right within the interior; the block straddling the fill edge is clipped
        // so the meter reads as "this much lit" rather than snapping to the next whole block. The
        // step guard keeps a zeroed SegmentWidth+SegmentGap from spinning the loop forever.
        double step = style.SegmentWidth + style.SegmentGap;
        if (step > 0) {
          for (double x = 0; x < fillWidth; x += step) {
            double blockRight = x + style.SegmentWidth;
            double drawWidth = (blockRight > fillWidth ? fillWidth : blockRight) - x;
            if (drawWidth > 0)
              dc.DrawRectangle(style.FillBrush, null,
                  new Rect(interior.X + x, interior.Y, drawWidth, interior.Height));
          }
        }
      }
      else {
        dc.DrawRectangle(style.FillBrush, null,
            new Rect(interior.X, interior.Y, fillWidth, interior.Height));
      }
    }
  }
}
