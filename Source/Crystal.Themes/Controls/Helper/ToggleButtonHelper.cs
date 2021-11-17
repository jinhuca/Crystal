namespace Crystal.Themes.Controls
{
  public static class ToggleButtonHelper
  {
    public static readonly DependencyProperty ContentDirectionProperty = DependencyProperty.RegisterAttached(
      "ContentDirection",
      typeof(FlowDirection),
      typeof(ToggleButtonHelper),
      new FrameworkPropertyMetadata(FlowDirection.LeftToRight));

    [AttachedPropertyBrowsableForType(typeof(ToggleButton))]
    [AttachedPropertyBrowsableForType(typeof(RadioButton))]
    [Category(AppName.CrystalThemes)]
    public static FlowDirection GetContentDirection(UIElement element) => (FlowDirection)element.GetValue(ContentDirectionProperty);

    public static void SetContentDirection(UIElement element, FlowDirection value) => element.SetValue(ContentDirectionProperty, value);
  }
}