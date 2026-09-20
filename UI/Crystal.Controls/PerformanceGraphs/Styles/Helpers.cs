using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Styles; 
internal static class Helpers {
  /// <summary>
  /// Creates a frozen pen with the specified brush and thickness. 
  /// Freezing the pen improves performance by making it immutable and thread-safe.
  /// </summary>
  /// <param name="brush">The brush to use for the pen.</param>
  /// <param name="thickness">The thickness of the pen.</param>
  /// <returns>The frozen pen.</returns>
  public static Pen CreateFrozenPen(Brush brush, double thickness) {
    var pen = new Pen(brush, thickness);
    pen.Freeze();
    return pen;
  }

  /// <summary>
  /// A thin dashed pen for reference/marker lines, distinct from the solid data line.
  /// </summary>
  /// <param name="brush">The brush to use for the pen.</param>
  /// <param name="thickness">The thickness of the pen.</param>
  /// <returns>The frozen pen.</returns>
  public static Pen CreateDashedPen(Brush brush, double thickness) {
    var pen = new Pen(brush, thickness) { DashStyle = new DashStyle([4, 3], 0) };
    pen.Freeze();
    return pen;
  }
}
