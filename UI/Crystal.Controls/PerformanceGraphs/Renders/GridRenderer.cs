using Crystal.Controls.PerformanceGraphs.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Renders;

internal sealed class GridRenderer {
  private readonly int _rows;
  private readonly int _columns;

  public GridRenderer(int rows, int columns) {
    _rows = Math.Max(1, rows);
    _columns = Math.Max(1, columns);
  }

  public void Draw(DrawingContext dc, Rect bounds, GraphStyle style) {
    if (_rows <= 0 || _columns <= 0) return;
    if (style.GridPen == null) return;

    double cellWidth = bounds.Width / _columns;
    double cellHeight = bounds.Height / _rows;

    // Vertical lines
    for (int col = 1; col < _columns; col++) {
      double x = bounds.Left + col * cellWidth;
      dc.DrawLine(style.GridPen, new Point(x, bounds.Top), new Point(x, bounds.Bottom));
    }

    // Horizontal lines
    for (int row = 1; row < _rows; row++) {
      double y = bounds.Top + row * cellHeight;
      dc.DrawLine(style.GridPen, new Point(bounds.Left, y), new Point(bounds.Right, y));
    }
  }
}