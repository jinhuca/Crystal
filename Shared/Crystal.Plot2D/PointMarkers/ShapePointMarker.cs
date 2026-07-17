using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.PointMarkers;

/// <summary>
/// Abstract class that extends PointMarker and contains marker property as OutlinePen, Brush and Diameter.
/// </summary>
public abstract class ShapePointMarker : PointMarker {
  /// <summary>
  /// Diameter of marker in points.
  /// </summary>
  public double Diameter {
    get => (double)GetValue(dp: DiameterProperty);
    set => SetValue(dp: DiameterProperty, value: value);
  }

  public static readonly DependencyProperty DiameterProperty = DependencyProperty.Register(
    name: nameof(Diameter),
    propertyType: typeof(double),
    ownerType: typeof(ShapePointMarker),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: 5.0));

  /// <summary>
  /// Pen for marker outline.
  /// </summary>
  public Pen OutlinePen {
    get => (Pen)GetValue(dp: OutlinePenProperty);
    set => SetValue(dp: OutlinePenProperty, value: value);
  }

  public static readonly DependencyProperty OutlinePenProperty = DependencyProperty.Register(
    name: nameof(OutlinePen),
    propertyType: typeof(Pen),
    ownerType: typeof(ShapePointMarker),
    typeMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: null));

  /// <summary>
  /// Brush to fill the marker.
  /// </summary>
  public Brush FillBrush {
    get => (Brush)GetValue(dp: FillBrushProperty);
    set => SetValue(dp: FillBrushProperty, value: value);
  }

  public static readonly DependencyProperty FillBrushProperty = DependencyProperty.Register(
    name: nameof(FillBrush),
    propertyType: typeof(Brush),
    ownerType: typeof(ShapePointMarker),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Transparent));
}
