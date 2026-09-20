using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Styles;

/// <summary>
/// Defines the visual style of a performance graph, including pens and brushes for borders, grids, lines, 
/// markers, fill, and background. This class allows customization of the appearance of the graph by 
/// providing properties to set the desired styles.
/// </summary>
internal sealed class GraphStyle {
  /// <summary>
  /// Initializes a new instance of the <see cref="GraphStyle"/> class with default styles for borders, 
  /// grids, lines, markers, fill, and background.
  /// </summary>
  public GraphStyle() {
    BorderPen = Helpers.CreateFrozenPen(Brushes.Black, 0.8);

    GridPen = Helpers.CreateFrozenPen(Brushes.Transparent, 0.6);
    LinePen = Helpers.CreateFrozenPen(Brushes.Blue, 1);

    FillBrush = Brushes.Transparent;
    BackgroundBrush = Brushes.Black;
  }

  /// <summary>
  /// Gets or sets the pen used to draw the border of the performance graph. 
  /// The border pen defines the color, thickness, and style of the border lines.
  /// </summary>
  public Pen BorderPen { get; set; }

  /// <summary>
  /// Gets or sets the pen used to draw the grid lines of the performance graph.
  /// </summary>
  public Pen GridPen { get; set; }

  /// <summary>
  /// Gets or sets the pen used to draw the lines of the performance graph.
  /// </summary>
  public Pen LinePen { get; set; }

  /// <summary>
  /// Null until a MarkerBrush is set on the control — no markers are drawn by default, so existing
  /// graphs are unaffected.
  /// </summary>
  public Pen? MarkerPen { get; set; }

  /// <summary>
  /// Gets or sets the brush used to fill the area under the performance graph line.
  /// </summary>
  public Brush FillBrush { get; set; }

  /// <summary>
  /// Gets or sets the brush used to fill the background of the performance graph.
  /// </summary>
  public Brush BackgroundBrush { get; set; }

  /// <summary>
  /// Gets or sets the thickness of the border lines of the performance graph. 
  /// This value is used to determine the width of the border when drawing the graph's outline.
  /// </summary>
  public double BorderThickness { get; set; } = 0.8;
}
