using Crystal.Plot2D.Common;
using System.Windows;

namespace Crystal.Plot2D.Charts;

// todo probably remove
public sealed class DataSource2dContext : DependencyObject {
  public static DataRect GetVisibleRect(DependencyObject obj)
    => (DataRect)obj.GetValue(dp: VisibleRectProperty);

  public static void SetVisibleRect(DependencyObject obj, DataRect value) {
    obj.SetValue(dp: VisibleRectProperty, value: value);
  }

  public static readonly DependencyProperty VisibleRectProperty = DependencyProperty.RegisterAttached(
    name: "VisibleRect",
    propertyType: typeof(DataRect),
    ownerType: typeof(DataSource2dContext),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: new DataRect()));

  public static Rect GetScreenRect(DependencyObject obj) => (Rect)obj.GetValue(dp: ScreenRectProperty);

  public static void SetScreenRect(DependencyObject obj, Rect value) => obj.SetValue(dp: ScreenRectProperty, value: value);

  public static readonly DependencyProperty ScreenRectProperty = DependencyProperty.RegisterAttached(
    name: "ScreenRect",
    propertyType: typeof(Rect),
    ownerType: typeof(DataSource2dContext),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: new Rect()));
}

