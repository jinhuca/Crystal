using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  /// <summary>
  /// Specifies the underline position of a TabControl.
  /// </summary>
  public enum UnderlinedType
  {
    None,
    TabItems,
    SelectedTabItem,
    TabPanel
  }

  public static class TabControlHelper
  {
    /// Sets the Style and Template property to null.
    ///
    /// Removing a TabItem in code behind can produce such nasty output
    /// System.Windows.Data Warning: 4 : Cannot find source for binding with reference 'RelativeSource FindAncestor, AncestorType='System.Windows.Controls.TabControl', AncestorLevel='1''. BindingExpression:Path=Background; DataItem=null; target element is 'TabItem' (Name=''); target property is 'Background' (type 'Brush')
    /// or
    /// System.Windows.Data Error: 4 : Cannot find source for binding with reference 'RelativeSource FindAncestor, AncestorType='System.Windows.Controls.TabControl', AncestorLevel='1''. BindingExpression:Path=(0); DataItem=null; target element is 'TabItem' (Name=''); target property is 'UnderlineBrush' (type 'Brush')
    ///
    /// This is a timing problem in WPF of the binding mechanism itself.
    ///
    /// To avoid this, we can set the Style and Template to null.
    public static void ClearStyle(this TabItem? tabItem)
    {
      if (tabItem is null)
      {
        return;
      }

      tabItem.Template = null;
      tabItem.Style = null;
    }

    public static readonly DependencyProperty CloseButtonEnabledProperty = DependencyProperty.RegisterAttached(
      "CloseButtonEnabled",
      typeof(bool),
      typeof(TabControlHelper),
      new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TabItem))]
    public static bool GetCloseButtonEnabled(UIElement element) => (bool)element.GetValue(CloseButtonEnabledProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TabItem))]
    public static void SetCloseButtonEnabled(UIElement element, bool value) => element.SetValue(CloseButtonEnabledProperty, BooleanBoxes.Box(value));

    public static readonly DependencyProperty CloseTabCommandProperty = DependencyProperty.RegisterAttached(
      "CloseTabCommand",
      typeof(ICommand),
      typeof(TabControlHelper),
      new PropertyMetadata(null));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TabItem))]
    public static ICommand? GetCloseTabCommand(UIElement element) => (ICommand?)element.GetValue(CloseTabCommandProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TabItem))]
    public static void SetCloseTabCommand(UIElement element, ICommand? value) => element.SetValue(CloseTabCommandProperty, value);

    public static readonly DependencyProperty CloseTabCommandParameterProperty = DependencyProperty.RegisterAttached(
      "CloseTabCommandParameter",
      typeof(object),
      typeof(TabControlHelper),
      new PropertyMetadata(null));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TabItem))]
    public static object? GetCloseTabCommandParameter(UIElement element) => element.GetValue(CloseTabCommandParameterProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TabItem))]
    public static void SetCloseTabCommandParameter(UIElement element, object? value) => element.SetValue(CloseTabCommandParameterProperty, value);

    public static readonly DependencyProperty UnderlinedProperty = DependencyProperty.RegisterAttached(
      "Underlined",
      typeof(UnderlinedType),
      typeof(TabControlHelper),
      new PropertyMetadata(UnderlinedType.None));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TabControl))]
    public static UnderlinedType GetUnderlined(UIElement element) => (UnderlinedType)element.GetValue(UnderlinedProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TabControl))]
    public static void SetUnderlined(UIElement element, UnderlinedType value)
    {
      element.SetValue(UnderlinedProperty, value);
    }

    public static readonly DependencyProperty UnderlineBrushProperty = DependencyProperty.RegisterAttached(
      "UnderlineBrush",
      typeof(Brush),
      typeof(TabControlHelper),
      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TabControl))]
    [AttachedPropertyBrowsableForType(typeof(TabItem))]
    public static Brush? GetUnderlineBrush(UIElement element) => (Brush?)element.GetValue(UnderlineBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TabControl))]
    [AttachedPropertyBrowsableForType(typeof(TabItem))]
    public static void SetUnderlineBrush(UIElement element, Brush? value) => element.SetValue(UnderlineBrushProperty, value);

    public static readonly DependencyProperty UnderlineSelectedBrushProperty = DependencyProperty.RegisterAttached(
      "UnderlineSelectedBrush",
      typeof(Brush),
      typeof(TabControlHelper),
      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TabControl))]
    [AttachedPropertyBrowsableForType(typeof(TabItem))]
    public static Brush? GetUnderlineSelectedBrush(UIElement element) => (Brush?)element.GetValue(UnderlineSelectedBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TabControl))]
    [AttachedPropertyBrowsableForType(typeof(TabItem))]
    public static void SetUnderlineSelectedBrush(UIElement element, Brush? value) => element.SetValue(UnderlineSelectedBrushProperty, value);

    public static readonly DependencyProperty UnderlineMouseOverBrushProperty = DependencyProperty.RegisterAttached(
      "UnderlineMouseOverBrush",
      typeof(Brush),
      typeof(TabControlHelper),
      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TabControl))]
    [AttachedPropertyBrowsableForType(typeof(TabItem))]
    public static Brush? GetUnderlineMouseOverBrush(UIElement element) => (Brush?)element.GetValue(UnderlineMouseOverBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TabControl))]
    [AttachedPropertyBrowsableForType(typeof(TabItem))]
    public static void SetUnderlineMouseOverBrush(UIElement element, Brush? value) => element.SetValue(UnderlineMouseOverBrushProperty, value);

    public static readonly DependencyProperty UnderlineMouseOverSelectedBrushProperty = DependencyProperty.RegisterAttached(
      "UnderlineMouseOverSelectedBrush",
      typeof(Brush),
      typeof(TabControlHelper),
      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TabControl))]
    [AttachedPropertyBrowsableForType(typeof(TabItem))]
    public static Brush? GetUnderlineMouseOverSelectedBrush(UIElement element) => (Brush?)element.GetValue(UnderlineMouseOverSelectedBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TabControl))]
    [AttachedPropertyBrowsableForType(typeof(TabItem))]
    public static void SetUnderlineMouseOverSelectedBrush(UIElement element, Brush? value) => element.SetValue(UnderlineMouseOverSelectedBrushProperty, value);

    public static readonly DependencyProperty TransitionProperty = DependencyProperty.RegisterAttached(
      "Transition",
      typeof(TransitionType),
      typeof(TabControlHelper),
      new FrameworkPropertyMetadata(TransitionType.Default, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    public static TransitionType GetTransition(DependencyObject obj) => (TransitionType)obj.GetValue(TransitionProperty);

    public static void SetTransition(DependencyObject obj, TransitionType value) => obj.SetValue(TransitionProperty, value);

    public static readonly DependencyProperty UnderlinePlacementProperty = DependencyProperty.RegisterAttached(
      "UnderlinePlacement",
      typeof(Dock?),
      typeof(TabControlHelper),
      new PropertyMetadata(null));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TabControl))]
    public static Dock? GetUnderlinePlacement(UIElement element) => (Dock?)element.GetValue(UnderlinePlacementProperty);

    public static void SetUnderlinePlacement(UIElement element, Dock? value) => element.SetValue(UnderlinePlacementProperty, value);
  }
}