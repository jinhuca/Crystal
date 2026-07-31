using System.Windows.Media;

namespace Crystal.Controls.MeterGauges.Styles;

internal static class Helpers {
  public static Pen CreateFrozenPen(Brush brush, double thickness) {
    var pen = new Pen(brush, (float)thickness);
    pen.Freeze();
    return pen;
  }
}
