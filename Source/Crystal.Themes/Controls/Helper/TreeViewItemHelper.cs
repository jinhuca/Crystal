namespace Crystal.Themes.Controls
{
  [StyleTypedProperty(Property = "ToggleButtonStyle", StyleTargetType = typeof(ToggleButton))]
  public static class TreeViewItemHelper
  {
    public static readonly DependencyProperty ToggleButtonStyleProperty = DependencyProperty.RegisterAttached(
      "ToggleButtonStyle",
      typeof(Style),
      typeof(TreeViewItemHelper),
      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static Style? GetToggleButtonStyle(UIElement element) 
      => (Style?)element.GetValue(ToggleButtonStyleProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static void SetToggleButtonStyle(UIElement element, Style? value) 
      => element.SetValue(ToggleButtonStyleProperty, value);
  }
}