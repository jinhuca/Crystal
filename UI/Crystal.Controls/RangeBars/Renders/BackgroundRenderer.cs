using Crystal.Controls.RangeBars.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.RangeBars.Renders;

/// <summary>
/// Renders the background of a <see cref="RangeBar"/> using the specified <see cref="RangeBarStyle"/>.
/// </summary>
internal sealed class BackgroundRenderer {
  /// <summary>
  /// Draws the background of a <see cref="RangeBar"/> using the specified <see cref="RangeBarStyle"/>.
  /// </summary>
  /// <param name="dc">The drawing context.</param>
  /// <param name="bounds">The bounds of the background.</param>
  /// <param name="style">The style to use for rendering.</param>
  public void Draw(DrawingContext dc, Rect bounds, RangeBarStyle style) {
    if (style.BackgroundBrush != null) {
      dc.DrawRectangle(style.BackgroundBrush, null, bounds);
    }
  }
}
