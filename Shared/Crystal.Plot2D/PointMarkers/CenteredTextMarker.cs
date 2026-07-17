using Crystal.Plot2D.Common.Auxiliary;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.PointMarkers;

/// <summary>
/// Renders specified text near the point
/// </summary>
public class CenteredTextMarker : PointMarker {
  public string Text {
    get => (string)GetValue(dp: TextProperty);
    set => SetValue(dp: TextProperty, value: value);
  }

  private const string TypefaceName = "Arial";
  public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
    name: nameof(Text),
    propertyType: typeof(string),
    ownerType: typeof(CenteredTextMarker),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: ""));

  public override void Render(DrawingContext dc, Point screenPoint) {
#pragma warning disable CS0618 // 'FormattedText.FormattedText(string, CultureInfo, FlowDirection, Typeface, double, Brush)' is obsolete: 'Use the PixelsPerDip override'
    FormattedText textToDraw = new(
      textToFormat: Text,
      culture: Thread.CurrentThread.CurrentCulture,
      flowDirection: FlowDirection.LeftToRight,
      typeface: new Typeface(typefaceName: TypefaceName), emSize: 12, foreground: Brushes.Black);
#pragma warning restore CS0618 // 'FormattedText.FormattedText(string, CultureInfo, FlowDirection, Typeface, double, Brush)' is obsolete: 'Use the PixelsPerDip override'

    var width = textToDraw.Width;
    var height = textToDraw.Height;

    const double verticalShift = -20; // px

    var bounds = RectExtensions.FromCenterSize(
      center: new Point(x: screenPoint.X, y: screenPoint.Y + verticalShift - height / 2),
      size: new Size(width: width, height: height));

    var loc = bounds.Location;
    bounds = CoordinateUtilities.RectZoom(rect: bounds, horizontalRatio: 1.05, verticalRatio: 1.15);

    dc.DrawLine(pen: new Pen(brush: Brushes.Black, thickness: 1), point0: Point.Add(point: screenPoint, vector: new Vector(x: 0, y: verticalShift)), point1: screenPoint);
    dc.DrawRectangle(brush: Brushes.White, pen: new Pen(brush: Brushes.Black, thickness: 1), rectangle: bounds);
    dc.DrawText(formattedText: textToDraw, origin: loc);
  }
}