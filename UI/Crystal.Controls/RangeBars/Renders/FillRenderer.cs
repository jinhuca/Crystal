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
    fraction = fraction < 0 ? 0 : (fraction > 1 ? 1 : fraction);

    // Inset by the border so the interior sits inside the stroke, which straddles the edge.
    double inset = style.BorderThickness / 2;
    Rect interior = new(
        bounds.X + inset,
        bounds.Y + inset,
        Math.Max(0, bounds.Width - style.BorderThickness),
        Math.Max(0, bounds.Height - style.BorderThickness));
    if (interior.Width <= 0 || interior.Height <= 0) return;

    if (style.TrackBrush != null) {
      dc.DrawRectangle(style.TrackBrush, null, interior);
    }

    double fillWidth = interior.Width * fraction;
    if (fillWidth > 0 && style.FillBrush != null) {
      dc.DrawRectangle(style.FillBrush, null,
          new Rect(interior.X, interior.Y, fillWidth, interior.Height));
    }
  }
}
