using Crystal.Controls.RangeBars.Styles;
using System;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.RangeBars.Renders;

internal sealed class BorderRenderer {
  public void Draw(DrawingContext dc, Rect bounds, RangeBarStyle style) {
    if (style.BorderThickness <= 0 || style.BorderPen == null) return;

    // Inset by half the stroke width so the (edge-straddling) border stays fully inside bounds
    // rather than being clipped at the control's edge. Clamp the deflated size to zero: the Rect
    // constructor throws on a negative width/height, which a control narrower/shorter than the
    // border thickness would otherwise produce.
    double inset = style.BorderThickness / 2;
    Rect rect = new(
        bounds.X + inset,
        bounds.Y + inset,
        Math.Max(0, bounds.Width - style.BorderThickness),
        Math.Max(0, bounds.Height - style.BorderThickness));
    if (rect.Width <= 0 || rect.Height <= 0) return;

    // First argument is the *fill* brush — pass null so this only strokes the outline.
    dc.DrawRectangle(null, style.BorderPen, rect);
  }
}
