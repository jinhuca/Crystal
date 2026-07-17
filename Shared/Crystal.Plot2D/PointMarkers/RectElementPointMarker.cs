using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Crystal.Plot2D.PointMarkers;

/// <summary>
///   Adds Circle element at every point of graph.
/// </summary>
public class RectElementPointMarker : ShapeElementPointMarker {
  internal override UIElement CreateMarker() {
    Rectangle result = new();
    result.Width = Size;
    result.Height = Size;
    result.Stroke = Brush;
    result.Fill = Fill;
    if(!string.IsNullOrEmpty(value: ToolTipText)) {
      result.ToolTip = new ToolTip {
        Content = ToolTipText
      };
    }
    return result;
  }

  internal override void SetMarkerProperties(UIElement marker) {
    var rect = (Rectangle)marker;

    rect.Width = Size;
    rect.Height = Size;
    rect.Stroke = Brush;
    rect.Fill = Fill;

    if(!string.IsNullOrEmpty(value: ToolTipText)) {
      rect.ToolTip = new ToolTip {
        Content = ToolTipText
      };
    }
  }

  internal override void SetPosition(UIElement marker, Point screenPoint) {
    Canvas.SetLeft(element: marker, length: screenPoint.X - Size / 2);
    Canvas.SetTop(element: marker, length: screenPoint.Y - Size / 2);
  }
}
