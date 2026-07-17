using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.PointMarkers;

public sealed class RectanglePointMarker : ShapePointMarker {
  public override void Render(DrawingContext dc, Point screenPoint) {
    var rec_ = new Rect(location: screenPoint, size: new Size(width: Diameter / 2, height: Diameter / 2));
    dc.DrawRectangle(brush: FillBrush, pen: OutlinePen, rectangle: rec_);
  }
}
