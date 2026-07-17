using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.PointMarkers;

/// <summary>
///   Abstract class that extends ElementPointMarker and contains marker property as OutlinePen, Brush and Diameter.
/// </summary>
public abstract class ShapeElementPointMarker : ElementPointMarker {
  /// <summary>
  ///   Diameter of marker in points.
  /// </summary>
  public double Size {
    get => (double)GetValue(dp: SizeProperty);
    set => SetValue(dp: SizeProperty, value: value);
  }

  public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
    name: "Diameter",
    propertyType: typeof(double),
    ownerType: typeof(ShapeElementPointMarker),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: 5.0));

  /// <summary>Tooltip to show when cursor on over</summary>
  public string ToolTipText {
    get => (string)GetValue(dp: ToolTipTextProperty);
    set => SetValue(dp: ToolTipTextProperty, value: value);
  }

  public static readonly DependencyProperty ToolTipTextProperty = DependencyProperty.Register(
    name: nameof(ToolTipText),
    propertyType: typeof(string),
    ownerType: typeof(ShapeElementPointMarker),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: string.Empty));

  /// <summary>OutlinePen to outline marker</summary>
  public Pen Pen {
    get => (Pen)GetValue(dp: PenProperty);
    set => SetValue(dp: PenProperty, value: value);
  }

  public static readonly DependencyProperty PenProperty = DependencyProperty.Register(
    name: "OutlinePen",
    propertyType: typeof(Pen),
    ownerType: typeof(ShapeElementPointMarker),
    typeMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: null));

  public Brush Brush {
    get => (Brush)GetValue(dp: BrushProperty);
    set => SetValue(dp: BrushProperty, value: value);
  }

  public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
    name: nameof(Brush),
    propertyType: typeof(Brush),
    ownerType: typeof(ShapeElementPointMarker),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Red));

  public Brush Fill {
    get => (Brush)GetValue(dp: FillProperty);
    set => SetValue(dp: FillProperty, value: value);
  }

  public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
    name: "FillBrush",
    propertyType: typeof(Brush),
    ownerType: typeof(ShapeElementPointMarker),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Red));
}