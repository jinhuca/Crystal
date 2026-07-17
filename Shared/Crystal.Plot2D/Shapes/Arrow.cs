using Crystal.Plot2D.Transforms;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.Shapes;

/// <summary>
///   Paints an arrow with start and end points in viewport coordinates.
/// </summary>
public class Arrow : Segment {
  /// <summary>
  ///   Initializes a new instance of the <see cref="Arrow"/> class.
  /// </summary>
  public Arrow() {
    Init();
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="Arrow"/> class.
  /// </summary>
  /// <param name="startPoint">
  ///   The start point of arrow.
  /// </param>
  /// <param name="endPoint">
  ///   The end point of arrow.
  /// </param>
  public Arrow(Point startPoint, Point endPoint) : base(startPoint: startPoint, endPoint: endPoint) {
    Init();
  }

  private void Init() {
    geometryGroup.Children.Add(value: LineGeometry);
    geometryGroup.Children.Add(value: leftLineGeometry);
    geometryGroup.Children.Add(value: rightLineGeometry);
  }

  #region ArrowLength property

  /// <summary>
  /// Gets or sets the length of the arrow.
  /// </summary>
  /// <value>The length of the arrow.</value>
  public double ArrowLength {
    get => (double)GetValue(dp: ArrowLengthProperty);
    set => SetValue(dp: ArrowLengthProperty, value: value);
  }

  /// <summary>
  /// Identifies ArrowLength dependency property.
  /// </summary>
  public static readonly DependencyProperty ArrowLengthProperty = DependencyProperty.Register(
    name: nameof(ArrowLength),
    propertyType: typeof(double),
    ownerType: typeof(Arrow),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: 0.1, propertyChangedCallback: OnPointChanged));

  #endregion

  #region ArrowAngle property

  /// <summary>
  /// Gets or sets the arrow angle in degrees.
  /// </summary>
  /// <value>The arrow angle.</value>
  public double ArrowAngle {
    get => (double)GetValue(dp: ArrowAngleProperty);
    set => SetValue(dp: ArrowAngleProperty, value: value);
  }

  /// <summary>
  /// Identifies ArrowAngle dependency property.
  /// </summary>
  public static readonly DependencyProperty ArrowAngleProperty = DependencyProperty.Register(
    name: nameof(ArrowAngle),
    propertyType: typeof(double),
    ownerType: typeof(Arrow),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: 15.0, propertyChangedCallback: OnPointChanged));

  #endregion

  protected override void UpdateUIRepresentationCore() {
    base.UpdateUIRepresentationCore();

    var transform = Plotter.Viewport.Transform;

    var p1 = StartPoint.DataToScreen(transform: transform);
    var p2 = EndPoint.DataToScreen(transform: transform);

    var arrowVector = p1 - p2;
    var arrowCapVector = ArrowLength * arrowVector;

    var leftMatrix = Matrix.Identity;
    leftMatrix.Rotate(angle: ArrowAngle);

    var rightMatrix = Matrix.Identity;
    rightMatrix.Rotate(angle: -ArrowAngle);

    var leftArrowLine = leftMatrix.Transform(vector: arrowCapVector);
    var rightArrowLine = rightMatrix.Transform(vector: arrowCapVector);

    leftLineGeometry.StartPoint = p2;
    rightLineGeometry.StartPoint = p2;

    leftLineGeometry.EndPoint = p2 + leftArrowLine;
    rightLineGeometry.EndPoint = p2 + rightArrowLine;
  }

  private readonly LineGeometry leftLineGeometry = new();
  private readonly LineGeometry rightLineGeometry = new();
  private readonly GeometryGroup geometryGroup = new();
  protected override Geometry DefiningGeometry => geometryGroup;
}
