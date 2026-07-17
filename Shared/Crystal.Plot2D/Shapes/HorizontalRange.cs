using Crystal.Plot2D.Transforms;
using System.Windows;

namespace Crystal.Plot2D.Shapes;

/// <summary>
/// Paints horizontal filled and outlined range in viewport coordinates.
/// </summary>
public sealed class HorizontalRange : RangeHighlight {
  protected override void UpdateUIRepresentationCore() {
    var transform = Plotter.Viewport.Transform;
    var visible = Plotter.Viewport.Visible;

    var p1_left = new Point(x: visible.XMin, y: StartValue).DataToScreen(transform: transform);
    var p1_right = new Point(x: visible.XMax, y: StartValue).DataToScreen(transform: transform);
    var p2_left = new Point(x: visible.XMin, y: EndValue).DataToScreen(transform: transform);
    var p2_right = new Point(x: visible.XMax, y: EndValue).DataToScreen(transform: transform);

    LineGeometry1.StartPoint = p1_left;
    LineGeometry1.EndPoint = p1_right;

    LineGeometry2.StartPoint = p2_left;
    LineGeometry2.EndPoint = p2_right;

    RectGeometry.Rect = new Rect(point1: p1_left, point2: p2_right);

  }
}
