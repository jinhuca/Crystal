using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Crystal.Plot2D.PointMarkers;

/// <summary>
/// Adds Circle element at every point of graph.
/// </summary>
public class CircleElementPointMarker : ShapeElementPointMarker {
  internal override UIElement CreateMarker() {
    Ellipse result = new();
    result.Width = Size;
    result.Height = Size;
    result.Stroke = Brush;
    result.Fill = Fill;

    if(!string.IsNullOrEmpty(value: ToolTipText)) {
      ToolTip tt = new();
      tt.Content = ToolTipText;
      result.ToolTip = tt;
    }
    return result;
  }

  internal override void SetMarkerProperties(UIElement marker) {
    var ellipse = (Ellipse)marker;

    ellipse.Width = Size;
    ellipse.Height = Size;
    ellipse.Stroke = Brush;
    ellipse.Fill = Fill;

    if(!string.IsNullOrEmpty(value: ToolTipText)) {
      ToolTip tt = new();
      tt.Content = ToolTipText;
      ellipse.ToolTip = tt;
    }
  }

  internal override void SetPosition(UIElement marker, Point screenPoint) {
    Canvas.SetLeft(element: marker, length: screenPoint.X - Size / 2);
    Canvas.SetTop(element: marker, length: screenPoint.Y - Size / 2);
  }
}