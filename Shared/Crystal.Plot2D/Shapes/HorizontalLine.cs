using Crystal.Plot2D.Transforms;
using System.Windows;

namespace Crystal.Plot2D.Shapes;

/// <summary>
///   Represents an infinite horizontal line with y-coordinate.
/// </summary>
public sealed class HorizontalLine : SimpleLine {
  /// <summary>
  ///   Initializes a new instance of the <see cref="HorizontalLine"/> class.
  /// </summary>
  public HorizontalLine() { }

  /// <summary>
  ///   Initializes a new instance of the <see cref="HorizontalLine"/> class with specified y coordinate.
  /// </summary>
  /// <param name="yCoordinate">
  ///   The y coordinate of line.
  /// </param>
  public HorizontalLine(double yCoordinate) {
    Value = yCoordinate;
  }

  protected override void UpdateUIRepresentationCore() {
    var transform = Plotter.Viewport.Transform;
    var p1 = new Point(x: Plotter.Viewport.Visible.XMin, y: Value).DataToScreen(transform: transform);
    var p2 = new Point(x: Plotter.Viewport.Visible.XMax, y: Value).DataToScreen(transform: transform);

    LineGeometry.StartPoint = p1;
    LineGeometry.EndPoint = p2;
  }
}
