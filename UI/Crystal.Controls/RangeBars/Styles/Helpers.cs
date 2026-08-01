using System.Windows.Media;

namespace Crystal.Controls.RangeBars.Styles;

internal static class Helpers {
  public static Pen CreateFrozenPen(Brush brush, double thickness) {
    var pen = new Pen(brush, thickness);
    pen.Freeze();
    return pen;
  }
}
