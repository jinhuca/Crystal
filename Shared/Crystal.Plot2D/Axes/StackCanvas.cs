using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Crystal.Plot2D.Axes;

internal sealed class StackCanvas : Panel {
  #region [-- EndCoordinate attached property --]

  [AttachedPropertyBrowsableForChildren]
  public static double GetEndCoordinate(DependencyObject obj) {
    return (double)obj.GetValue(dp: EndCoordinateProperty);
  }

  public static void SetEndCoordinate(DependencyObject obj, double value) {
    obj.SetValue(dp: EndCoordinateProperty, value: value);
  }

  public static readonly DependencyProperty EndCoordinateProperty = DependencyProperty.RegisterAttached(
    name: "EndCoordinate",
    propertyType: typeof(double),
    ownerType: typeof(StackCanvas),
    defaultMetadata: new PropertyMetadata(defaultValue: double.NaN, propertyChangedCallback: OnCoordinateChanged));

  #endregion [-- EndCoordinate attached property --]

  #region [-- Coordinate attached property --]

  [AttachedPropertyBrowsableForChildren]
  public static double GetCoordinate(DependencyObject obj) {
    return (double)obj.GetValue(dp: CoordinateProperty);
  }

  public static void SetCoordinate(DependencyObject obj, double value) {
    obj.SetValue(dp: CoordinateProperty, value: value);
  }

  public static readonly DependencyProperty CoordinateProperty = DependencyProperty.RegisterAttached(
    name: "Coordinate",
    propertyType: typeof(double),
    ownerType: typeof(StackCanvas),
    defaultMetadata: new PropertyMetadata(defaultValue: 0.0, propertyChangedCallback: OnCoordinateChanged));

  private static void OnCoordinateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if(d is UIElement reference) {
      if(VisualTreeHelper.GetParent(reference: reference) is StackCanvas parent) {
        parent.InvalidateArrange();
      }
    }
  }

  #endregion [-- Coordinate attached property --]

  #region [-- AxisPlacement property --]

  public AxisPlacement Placement {
    get => (AxisPlacement)GetValue(dp: PlacementProperty);
    set => SetValue(dp: PlacementProperty, value: value);
  }

  public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
    name: nameof(Placement),
    propertyType: typeof(AxisPlacement),
    ownerType: typeof(StackCanvas),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: AxisPlacement.Bottom, flags: FrameworkPropertyMetadataOptions.AffectsArrange));

  #endregion [-- AxisPlacement property --]

  private bool IsHorizontal => Placement == AxisPlacement.Top || Placement == AxisPlacement.Bottom;

  protected override Size MeasureOverride(Size constraint) {
    var availableSize = constraint;
    Size size = new();

    var isHorizontal = IsHorizontal;

    if(isHorizontal) {
      availableSize.Width = double.PositiveInfinity;
      size.Width = constraint.Width;
    }
    else {
      availableSize.Height = double.PositiveInfinity;
      size.Height = constraint.Height;
    }

    // measuring all children and determining self width and height
    foreach(UIElement element in Children) {
      if(element != null) {
        var childSize = GetChildSize(element: element, availableSize: availableSize);
        element.Measure(availableSize: childSize);
        var desiredSize = element.DesiredSize;

        if(isHorizontal) {
          size.Height = Math.Max(val1: size.Height, val2: desiredSize.Height);
        }
        else {
          size.Width = Math.Max(val1: size.Width, val2: desiredSize.Width);
        }
      }
    }

    if(double.IsPositiveInfinity(d: size.Width)) {
      size.Width = 0;
    }

    if(double.IsPositiveInfinity(d: size.Height)) {
      size.Height = 0;
    }

    return size;
  }

  private Size GetChildSize(UIElement element, Size availableSize) {
    var coordinate = GetCoordinate(obj: element);
    var endCoordinate = GetEndCoordinate(obj: element);

    if(coordinate.IsNotNaN() && endCoordinate.IsNotNaN()) {
      if(Placement.IsBottomOrTop()) {
        availableSize.Width = endCoordinate - coordinate;
      }
      else {
        availableSize.Height = Math.Abs(value: endCoordinate - coordinate);
      }
    }

    return availableSize;
  }

  protected override Size ArrangeOverride(Size finalSize) {
    var isHorizontal = IsHorizontal;

    foreach(FrameworkElement element in Children) {
      if(element == null) {
        continue;
      }

      var elementSize = element.DesiredSize;
      var x = 0.0;
      var y = 0.0;

      switch(Placement) {
        case AxisPlacement.Left:
          x = finalSize.Width - elementSize.Width;
          break;
        case AxisPlacement.Right:
          x = 0;
          break;
        case AxisPlacement.Top:
          y = finalSize.Height - elementSize.Height;
          break;
        case AxisPlacement.Bottom:
          y = 0;
          break;
      }

      var coordinate = GetCoordinate(obj: element);

      if(!double.IsNaN(d: GetEndCoordinate(obj: element))) {
        var endCoordinate = GetEndCoordinate(obj: element);
        var size = endCoordinate - coordinate;
        if(size < 0) {
          size = -size;
          coordinate -= size;
        }
        if(isHorizontal) {
          elementSize.Width = size;
        }
        else {
          elementSize.Height = size;
        }
      }

      // shift for common tick labels, not for major ones.
      if(isHorizontal) {
        x = coordinate;
        if(element.HorizontalAlignment == HorizontalAlignment.Center) {
          x = coordinate - elementSize.Width / 2;
        }
      }
      else {
        if(element.VerticalAlignment == VerticalAlignment.Center) {
          y = coordinate - elementSize.Height / 2;
        }
        else if(element.VerticalAlignment == VerticalAlignment.Bottom) {
          y = coordinate - elementSize.Height;
        }
        else if(element.VerticalAlignment == VerticalAlignment.Top) {
          y = coordinate;
        }
      }

      Rect bounds = new(location: new Point(x: x, y: y), size: elementSize);
      element.Arrange(finalRect: bounds);
    }

    return finalSize;
  }
}