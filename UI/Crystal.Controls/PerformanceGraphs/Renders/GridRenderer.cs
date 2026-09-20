using Crystal.Controls.PerformanceGraphs.Styles;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Renders;

/// <summary>
/// Renders the grid lines of a performance graph, drawing horizontal and vertical lines within the 
/// specified bounds based on the number of rows and columns defined at construction. The grid lines 
/// are drawn using the grid pen defined in the graph style.
/// </summary>
internal sealed class GridRenderer {
  /// <summary>
  /// The number of rows in the grid. Must be greater than zero.
  /// </summary>
  private readonly int _rows;

  /// <summary>
  /// The number of columns in the grid. Must be greater than zero.
  /// </summary>
  private readonly int _columns;

  /// <summary>
  /// The grid lines depend only on the plot size (row/column counts are fixed at construction), yet
  /// OnRender re-runs on every sample. Building rows+columns line segments each frame — for graphs
  /// that show a grid — is pure waste, so the paths are batched into one geometry and rebuilt only
  /// when the size changes. A single frozen geometry drawn once per frame replaces the per-frame
  /// rows+columns DrawLine calls (and the per-frame instructions retained on the visual).
  /// </summary>
  private Geometry? _geometry;

  /// <summary>
  /// The cached width of the bounds used to build the geometry. 
  /// If the current bounds width differs from this value, the geometry will be rebuilt.
  /// </summary>
  private double _cachedWidth = double.NaN;

  /// <summary>
  /// The cached height of the bounds used to build the geometry.
  /// </summary>
  private double _cachedHeight = double.NaN;

  /// <summary>
  /// Initializes a new instance of the <see cref="GridRenderer"/> class.
  /// </summary>
  /// <param name="rows">The number of rows in the grid. Must be greater than zero.</param>
  /// <param name="columns">The number of columns in the grid. Must be greater than zero.</param>
  public GridRenderer(int rows, int columns) {
    _rows = Math.Max(1, rows);
    _columns = Math.Max(1, columns);
  }

  /// <summary>
  /// Draws the grid lines within the specified bounds using the provided drawing context and graph style.
  /// </summary>
  /// <param name="dc">The drawing context.</param>
  /// <param name="bounds">The bounds within which to draw the grid lines.</param>
  /// <param name="style">The graph style containing the grid pen.</param>
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

  /// <summary>
  /// One geometry holding every interior grid line as an open, stroked figure. Frozen so WPF can
  /// render it without per-frame cloning; a size change discards it and builds a fresh one.
  /// </summary>
  /// <param name="bounds">The bounds within which to build the geometry.</param>
  /// <returns>The built geometry.</returns>
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
