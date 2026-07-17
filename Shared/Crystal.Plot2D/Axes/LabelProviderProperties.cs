using System.Windows;

namespace Crystal.Plot2D.Axes;

internal sealed class LabelProviderProperties : DependencyObject {
  public static bool GetExponentialIsCommonLabel(DependencyObject obj) {
    return (bool)obj.GetValue(dp: ExponentialIsCommonLabelProperty);
  }

  public static void SetExponentialIsCommonLabel(DependencyObject obj, bool value) {
    obj.SetValue(dp: ExponentialIsCommonLabelProperty, value: value);
  }

  public static readonly DependencyProperty ExponentialIsCommonLabelProperty = DependencyProperty.RegisterAttached(
    name: "ExponentialIsCommonLabel",
    propertyType: typeof(bool),
    ownerType: typeof(LabelProviderProperties),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: true));
}
