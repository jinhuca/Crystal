using Crystal.Plot2D.Transforms;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.Shapes;

/// <summary>
///   Represents a polyline with points in Viewport coordinates.
/// </summary>
public sealed class ViewportPolyline : ViewportPolylineBase {
  /// <summary>
  ///   Initializes a new instance of the <see cref="ViewportPolyline"/> class.
  /// </summary>
  public ViewportPolyline() { }

  protected override void UpdateUIRepresentationCore() {
    var transform_ = Plotter.Viewport.Transform;
    var geometry_ = PathGeometry;
    var points_ = Points;
    geometry_.Clear();

    if(points_ == null) { }
    else {
      PathFigure figure_ = new();
      if(points_.Count > 0) {
        figure_.StartPoint = points_[index: 0].DataToScreen(transform: transform_);
        if(points_.Count > 1) {
          var pointArray_ = new Point[points_.Count - 1];
          for(var i_ = 1; i_ < points_.Count; i_++) {
            pointArray_[i_ - 1] = points_[index: i_].DataToScreen(transform: transform_);
          }

          figure_.Segments.Add(value: new PolyLineSegment(points: pointArray_, isStroked: true));
        }
      }

      geometry_.Figures.Add(value: figure_);
      geometry_.FillRule = FillRule;
    }
  }
}
