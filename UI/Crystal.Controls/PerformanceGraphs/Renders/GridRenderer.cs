using Crystal.Controls.PerformanceGraphs.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Renders;

internal sealed class GridRenderer {
  private readonly int _rows;
  private readonly int _columns;

  // The grid lines depend only on the plot size (row/column counts are fixed at construction), yet
  // OnRender re-runs on every sample. Building rows+columns line segments each frame — for graphs
  // that show a grid — is pure waste, so the paths are batched into one geometry and rebuilt only
  // when the size changes. A single frozen geometry drawn once per frame replaces the per-frame
  // rows+columns DrawLine calls (and the per-frame instructions retained on the visual).
  private Geometry? _geometry;
  private double _cachedWidth = double.NaN;
  private double _cachedHeight = double.NaN;

  public GridRenderer(int rows, int columns) {
    _rows = Math.Max(1, rows);
    _columns = Math.Max(1, columns);
  }

  public void Draw(DrawingContext dc, Rect bounds, GraphStyle style) {
    if (_rows <= 0 || _columns <= 0) return;
    if (style.GridPen == null) return;
    // No visible grid (e.g. the No-Frills category zeroes the thickness) — skip the whole
    // line-drawing loop rather than issuing rows+columns invisible DrawLine calls every frame.
    if (style.GridPen.Thickness <= 0) return;
    if (bounds.Width <= 0 || bounds.Height <= 0) return;

    if (_geometry == null || bounds.Width != _cachedWidth || bounds.Height != _cachedHeight) {
      _geometry = BuildGeometry(bounds);
      _cachedWidth = bounds.Width;
      _cachedHeight = bounds.Height;
    }

    // The pen is applied at draw time, so a grid-brush/thickness change (e.g. toggling the category)
    // needs no geometry rebuild — only a size change does.
    dc.DrawGeometry(null, style.GridPen, _geometry);
  }

  // One geometry holding every interior grid line as an open, stroked figure. Frozen so WPF can
  // render it without per-frame cloning; a size change discards it and builds a fresh one.
  private Geometry BuildGeometry(Rect bounds) {
    double cellWidth = bounds.Width / _columns;
    double cellHeight = bounds.Height / _rows;

    var geometry = new StreamGeometry();
    using (StreamGeometryContext ctx = geometry.Open()) {
      for (int col = 1; col < _columns; col++) {
        double x = bounds.Left + col * cellWidth;
        ctx.BeginFigure(new Point(x, bounds.Top), isFilled: false, isClosed: false);
        ctx.LineTo(new Point(x, bounds.Bottom), isStroked: true, isSmoothJoin: false);
      }
      for (int row = 1; row < _rows; row++) {
        double y = bounds.Top + row * cellHeight;
        ctx.BeginFigure(new Point(bounds.Left, y), isFilled: false, isClosed: false);
        ctx.LineTo(new Point(bounds.Right, y), isStroked: true, isSmoothJoin: false);
      }
    }
    geometry.Freeze();
    return geometry;
  }
}
