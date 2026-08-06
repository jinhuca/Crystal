using Crystal.Controls.MeterGauges.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.MeterGauges.Renders;

internal sealed class BackgroundRenderer {
  public void Draw(DrawingContext dc, Rect bounds, GaugeStyle style) {
    if (style.BackgroundBrush != null) {
      dc.DrawRectangle(style.BackgroundBrush, null, bounds);
    }
  }
}
