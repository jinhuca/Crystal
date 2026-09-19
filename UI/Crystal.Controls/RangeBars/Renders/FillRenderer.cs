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

    // When an accent color is set, paint with a gradient that spans the *full* interior
    // (min→max), so any partial rectangle drawn on top shows the slice of the gradient at its
    // position — the alpha at the fill edge maps to the current value. Absolute mapping keeps
    // that mapping stable whether the fill is solid or split into LED blocks.
    Brush fillBrush = style.AccentColor is Color accent
        ? BuildGradient(accent, style, interior)
        : style.FillBrush;

    if (fillBrush == null) return;

    if (style.Segmented && style.SegmentCount > 0) {
      DrawSquares(dc, interior, style, fillBrush, fraction);
      return;
    }

    double fillWidth = interior.Width * fraction;
    if (fillWidth > 0) {
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
              dc.DrawRectangle(fillBrush, null,
                  new Rect(interior.X + x, interior.Y, drawWidth, interior.Height));
          }
        }
      }
      else {
        dc.DrawRectangle(fillBrush, null,
            new Rect(interior.X, interior.Y, fillWidth, interior.Height));
      }
    }
  }

  /// <summary>
  /// Draws exactly <see cref="RangeBarStyle.SegmentCount"/> squares (side = bar height, so each is
  /// a square not a rectangle) evenly spaced across the full scale. The meter fills per square
  /// against the value fraction — so <c>count·fraction</c> squares light up and the block at the
  /// value edge shows only its fractional part (e.g. 5.5 squares at value 55 of 0..100 with 10).
  /// </summary>
  private static void DrawSquares(DrawingContext dc, Rect interior, RangeBarStyle style, Brush fillBrush, double fraction) {
    int count = style.SegmentCount;
    double pitch = interior.Width / count;
    // Keep each block square: side is the bar height, capped to the cell pitch when the bar is too
    // narrow to space that many squares out. Center it vertically when capped.
    double side = Math.Min(interior.Height, pitch);
    double top = interior.Y + (interior.Height - side) / 2;

    double litSquares = fraction * count;
    int full = (int)Math.Floor(litSquares);
    double partial = litSquares - full;

    for (int i = 0; i < count; i++) {
      double width;
      if (i < full) width = side;
      else if (i == full && partial > 0) width = side * partial;
      else break;

      dc.DrawRectangle(fillBrush, null, new Rect(interior.X + i * pitch, top, width, side));
    }
  }

  /// <summary>
  /// Builds a horizontal <see cref="LinearGradientBrush"/> over <paramref name="interior"/> where
  /// only the accent color's alpha varies — from <see cref="RangeBarStyle.LowestAlpha"/> to
  /// <see cref="RangeBarStyle.HighestAlpha"/> (or the reverse for
  /// <see cref="RangeBarGradientDirection.Descending"/>). Absolute mapping pins the stops to the
  /// interior edges so partial fills and LED blocks sample the correct slice.
  /// </summary>
  private static Brush BuildGradient(Color accent, RangeBarStyle style, Rect interior) {
    Color low = Color.FromArgb(style.LowestAlpha, accent.R, accent.G, accent.B);
    Color high = Color.FromArgb(style.HighestAlpha, accent.R, accent.G, accent.B);

    (Color start, Color end) = style.Direction == RangeBarGradientDirection.Descending
        ? (high, low)
        : (low, high);

    var brush = new LinearGradientBrush {
      MappingMode = BrushMappingMode.Absolute,
      StartPoint = new Point(interior.X, interior.Y),
      EndPoint = new Point(interior.Right, interior.Y)
    };
    brush.GradientStops.Add(new GradientStop(start, 0));
    brush.GradientStops.Add(new GradientStop(end, 1));
    brush.Freeze();
    return brush;
  }
}
