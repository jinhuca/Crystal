using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Styles; 
internal static class Helpers {
  public static Pen CreateFrozenPen(Brush brush, double thickness) {
    var pen = new Pen(brush, thickness);
    pen.Freeze();
    return pen;
  }

  // A thin dashed pen for reference/marker lines, distinct from the solid data line.
  public static Pen CreateDashedPen(Brush brush, double thickness) {
    var pen = new Pen(brush, thickness) { DashStyle = new DashStyle([4, 3], 0) };
    pen.Freeze();
    return pen;
  }
}
