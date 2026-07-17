using Crystal.Plot2D.Transforms;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.Shapes;

/// <summary>
/// Represents a segment with start and end points bound to viewport coordinates.
/// </summary>
public class Segment : ViewportShape {
  /// <summary>
  /// Initializes a new instance of the <see cref="Segment"/> class.
  /// </summary>
  public Segment() {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="Segment"/> class.
  /// </summary>
  /// <param name="startPoint">The start point of segment.</param>
  /// <param name="endPoint">The end point of segment.</param>
  public Segment(Point startPoint, Point endPoint) {
    StartPoint = startPoint;
    EndPoint = endPoint;
  }

  /// <summary>
  /// Gets or sets the start point of segment.
  /// </summary>
  /// <value>The start point.</value>
  public Point StartPoint {
    get => (Point)GetValue(dp: StartPointProperty);
    set => SetValue(dp: StartPointProperty, value: value);
  }

  /// <summary>
  /// Identifies the StartPoint dependency property.
  /// </summary>
  public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register(
    name: nameof(StartPoint),
    propertyType: typeof(Point),
    ownerType: typeof(Segment),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: new Point(), propertyChangedCallback: OnPointChanged));

  protected static void OnPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var segment_ = (Segment)d;
    segment_.UpdateUIRepresentation();
  }

  protected void OnPointChanged() {
  }

  /// <summary>
  /// Gets or sets the end point of segment.
  /// </summary>
  /// <value>The end point.</value>
  public Point EndPoint {
    get => (Point)GetValue(dp: EndPointProperty);
    set => SetValue(dp: EndPointProperty, value: value);
  }

  /// <summary>
  /// Identifies the EndPoint dependency property.
  /// </summary>
  public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register(
    name: nameof(EndPoint),
    propertyType: typeof(Point),
    ownerType: typeof(Segment),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: new Point(), propertyChangedCallback: OnPointChanged));

  protected override void UpdateUIRepresentationCore() {
    if(Plotter == null) {
      return;
    }

    var transform_ = Plotter.Viewport.Transform;
    var p1_ = StartPoint.DataToScreen(transform: transform_);
    var p2_ = EndPoint.DataToScreen(transform: transform_);

    lineGeometry.StartPoint = p1_;
    lineGeometry.EndPoint = p2_;
  }

  private readonly LineGeometry lineGeometry = new();
  protected LineGeometry LineGeometry => lineGeometry;

  protected override Geometry DefiningGeometry => lineGeometry;
}
