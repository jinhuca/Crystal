namespace Crystal.Themes.Controls;

public static class ValidationHelper
{
  public static readonly DependencyProperty CloseOnMouseLeftButtonDownProperty = DependencyProperty.RegisterAttached(
    "CloseOnMouseLeftButtonDown",
    typeof(bool),
    typeof(ValidationHelper),
    new PropertyMetadata(BooleanBoxes.FalseBox));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(UIElement))]
  public static bool GetCloseOnMouseLeftButtonDown(UIElement element) => (bool)element.GetValue(CloseOnMouseLeftButtonDownProperty);

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(UIElement))]
  public static void SetCloseOnMouseLeftButtonDown(UIElement element, bool value) => element.SetValue(CloseOnMouseLeftButtonDownProperty, BooleanBoxes.Box(value));

  public static readonly DependencyProperty ShowValidationErrorOnMouseOverProperty = DependencyProperty.RegisterAttached(
    "ShowValidationErrorOnMouseOver",
    typeof(bool),
    typeof(ValidationHelper),
    new PropertyMetadata(BooleanBoxes.FalseBox));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(UIElement))]
  public static bool GetShowValidationErrorOnMouseOver(UIElement element) => (bool)element.GetValue(ShowValidationErrorOnMouseOverProperty);

  /// <summary>
  /// Sets whether the validation error text will be shown when hovering the validation triangle.
  /// </summary>
  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(UIElement))]
  public static void SetShowValidationErrorOnMouseOver(UIElement element, bool value) => element.SetValue(ShowValidationErrorOnMouseOverProperty, BooleanBoxes.Box(value));
}