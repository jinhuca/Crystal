using Crystal.Plot2D.Transforms;
using System.Windows;

namespace Crystal.Plot2D.Shapes;

/// <inheritdoc />
/// <summary>
/// Represents an infinite vertical line with x viewport coordinate.
/// </summary>
public sealed class VerticalLine : SimpleLine {
  /// <inheritdoc />
  /// <summary>
  /// Initializes a new instance of the <see cref="T:Crystal.Plot2D.Shapes.VerticalLine" /> class.
  /// </summary>
  public VerticalLine() {
  }

  protected override void UpdateUIRepresentationCore() {
    var transform_ = Plotter.Viewport.Transform;
    var p1_ = new Point(x: Value, y: Plotter.Viewport.Visible.YMin).DataToScreen(transform: transform_);
    var p2_ = new Point(x: Value, y: Plotter.Viewport.Visible.YMax).DataToScreen(transform: transform_);

    LineGeometry.StartPoint = p1_;
    LineGeometry.EndPoint = p2_;
  }
}
