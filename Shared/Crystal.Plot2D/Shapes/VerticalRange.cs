using Crystal.Plot2D.Transforms;
using System.Windows;

namespace Crystal.Plot2D.Shapes;

/// <summary>
/// Paints vertical filled and outlined range in viewport coordinates.
/// </summary>
public sealed class VerticalRange : RangeHighlight {
  protected override void UpdateUIRepresentationCore() {
    var transform_ = Plotter.Viewport.Transform;
    var visible_ = Plotter.Viewport.Visible;

    var p1Top_ = new Point(x: StartValue, y: visible_.YMin).DataToScreen(transform: transform_);
    var p1Bottom_ = new Point(x: StartValue, y: visible_.YMax).DataToScreen(transform: transform_);
    var p2Top_ = new Point(x: EndValue, y: visible_.YMin).DataToScreen(transform: transform_);
    var p2Bottom_ = new Point(x: EndValue, y: visible_.YMax).DataToScreen(transform: transform_);

    LineGeometry1.StartPoint = p1Top_;
    LineGeometry1.EndPoint = p1Bottom_;

    LineGeometry2.StartPoint = p2Top_;
    LineGeometry2.EndPoint = p2Bottom_;

    RectGeometry.Rect = new Rect(point1: p1Top_, point2: p2Bottom_);
  }
}
