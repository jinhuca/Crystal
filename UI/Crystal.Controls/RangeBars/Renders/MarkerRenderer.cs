using Crystal.Controls.RangeBars.Styles;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.RangeBars.Renders;

/// <summary>
/// Draws <see cref="RangeBar.Markers"/>: a vertical tick at each marker's value position (mapped
/// through the same border-inset interior the fill uses, so ticks line up with the fill edge), plus
/// an optional caption in the bar's label band below. Captions are centered on their tick, clamped
/// to stay within the control, and shifted down by <see cref="RangeBarMarker.LabelLine"/> rows so
/// neighbors that would overlap can stack instead.
/// </summary>
internal sealed class MarkerRenderer {
  private static readonly Typeface LabelTypeface =
      new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

  public void Draw(DrawingContext dc, Rect barBounds, Rect fullBounds, RangeBarStyle style,
      IEnumerable<RangeBarMarker>? markers, double min, double max, double labelFontSize, double pixelsPerDip) {
    if (markers == null) return;

    double range = max - min;
    if (range <= 0) return;

    Rect interior = Helpers.Deflate(barBounds, style.BorderThickness);
    if (interior.Width <= 0 || interior.Height <= 0) return;

    double lineHeight = labelFontSize * 1.35;

    foreach (RangeBarMarker marker in markers) {
      if (marker == null) continue;

      double fraction = (marker.Value - min) / range;
      if (fraction < 0 || fraction > 1) continue; // off-scale: don't draw outside the track

      double x = interior.X + fraction * interior.Width;
      Brush tickBrush = marker.Brush ?? Brushes.Gray;

      double top = barBounds.Top - marker.Overhang;
      double height = barBounds.Height + 2 * marker.Overhang;
      dc.DrawRectangle(tickBrush, null, new Rect(x - marker.Thickness / 2, top, marker.Thickness, height));

      if (string.IsNullOrEmpty(marker.Label)) continue;

      var text = new FormattedText(marker.Label, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
          LabelTypeface, labelFontSize, marker.LabelBrush ?? tickBrush, pixelsPerDip);

      double labelX = x - text.Width / 2;
      if (labelX < fullBounds.Left) labelX = fullBounds.Left;
      if (labelX + text.Width > fullBounds.Right) labelX = fullBounds.Right - text.Width;

      double labelY = barBounds.Bottom + 2 + marker.LabelLine * lineHeight;
      dc.DrawText(text, new Point(labelX, labelY));
    }
  }
}
