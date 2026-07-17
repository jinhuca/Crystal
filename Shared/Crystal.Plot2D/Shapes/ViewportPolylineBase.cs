using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.Shapes;

public abstract class ViewportPolylineBase : ViewportShape {
  #region Properties

  /// <summary>
  /// Gets or sets the points in Viewport coordinates, that form the line.
  /// </summary>
  /// <value>The points.</value>
  public PointCollection Points {
    get => (PointCollection)GetValue(dp: PointsProperty);
    set => SetValue(dp: PointsProperty, value: value);
  }

  /// <summary>
  /// Identifies the Points dependency property.
  /// </summary>
  public static readonly DependencyProperty PointsProperty = DependencyProperty.Register(
    name: nameof(Points),
    propertyType: typeof(PointCollection),
    ownerType: typeof(ViewportPolylineBase),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: new PointCollection(), propertyChangedCallback: OnPropertyChanged));

  protected static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var polyline_ = (ViewportPolylineBase)d;
    var currentPoints_ = (PointCollection)e.NewValue;
    polyline_.UpdateUIRepresentation();
  }

  /// <summary>
  /// Gets or sets the fill rule of polygon or polyline.
  /// </summary>
  /// <value>The fill rule.</value>
  public FillRule FillRule {
    get => (FillRule)GetValue(dp: FillRuleProperty);
    set => SetValue(dp: FillRuleProperty, value: value);
  }

  public static readonly DependencyProperty FillRuleProperty = DependencyProperty.Register(
    name: nameof(FillRule),
    propertyType: typeof(FillRule),
    ownerType: typeof(ViewportPolylineBase),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: FillRule.EvenOdd, propertyChangedCallback: OnPropertyChanged));

  #endregion

  private readonly PathGeometry geometry = new();
  protected PathGeometry PathGeometry => geometry;

  protected sealed override Geometry DefiningGeometry => geometry;
}
