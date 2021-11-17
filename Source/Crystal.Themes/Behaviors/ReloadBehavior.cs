using Crystal.Themes.Controls;
using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Behaviors
{
  public static class ReloadBehavior
  {
    /// <summary>
    /// The DependencyProperty for the <see cref="CrystalContentControl"/>' OnDataContextChanged property.
    ///
    /// With the OnDataContextChanged property the Reload behavior of the CrystalContentControl can be switched on or off.
    /// If the property is set to true, the transition of the <see cref="CrystalContentControl"/> is triggered again when the DataContext is changed.
    /// </summary>
    public static readonly DependencyProperty OnDataContextChangedProperty = DependencyProperty.RegisterAttached(
      "OnDataContextChanged",
      typeof(bool),
      typeof(ReloadBehavior),
      new PropertyMetadata(BooleanBoxes.FalseBox, OnOnDataContextChanged));

    /// <summary>
    /// Helper for getting <see cref="OnDataContextChangedProperty"/> from <paramref name="element"/>.
    ///
    /// If the property is set to true, the transition of the <see cref="CrystalContentControl"/> is triggered again when the DataContext is changed.
    /// </summary>
    /// <param name="element"><see cref="UIElement"/> to read <see cref="OnDataContextChangedProperty"/> from.</param>
    /// <returns>OnDataContextChanged property value.</returns>
    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(CrystalContentControl))]
    public static bool GetOnDataContextChanged(UIElement element)
    {
      return (bool)element.GetValue(OnDataContextChangedProperty);
    }

    /// <summary>
    /// Helper for setting <see cref="OnDataContextChangedProperty"/> on <paramref name="element"/>.
    ///
    /// If the property is set to true, the transition of the <see cref="CrystalContentControl"/> is triggered again when the DataContext is changed.
    /// </summary>
    /// <param name="element"><see cref="UIElement"/> to set <see cref="OnDataContextChangedProperty"/> on.</param>
    /// <param name="value">OnDataContextChanged property value.</param>
    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(CrystalContentControl))]
    public static void SetOnDataContextChanged(UIElement element, bool value)
    {
      element.SetValue(OnDataContextChangedProperty, BooleanBoxes.Box(value));
    }

    private static void OnOnDataContextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue != e.NewValue && dependencyObject is CrystalContentControl crystalContentControl)
      {
        crystalContentControl.DataContextChanged -= ReloadOnDataContextChanged;
        crystalContentControl.DataContextChanged += ReloadOnDataContextChanged;
      }
    }

    private static void ReloadOnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      (sender as CrystalContentControl)?.Reload();
    }

    /// <summary>
    /// The DependencyProperty for the <see cref="CrystalContentControl"/>' and <see cref="TransitioningContentControl"/>' OnSelectedTabChanged property.
    ///
    /// With the OnSelectedTabChanged property the Reload behavior of the control can be switched on or off.
    /// If the property is set to true, the transition is triggered again when the SelectionChanged event of a TabControl was raised.
    /// </summary>
    public static readonly DependencyProperty OnSelectedTabChangedProperty
        = DependencyProperty.RegisterAttached(
            "OnSelectedTabChanged",
            typeof(bool),
            typeof(ReloadBehavior),
            new PropertyMetadata(BooleanBoxes.FalseBox, OnSelectedTabChanged));

    /// <summary>
    /// Helper for getting <see cref="OnSelectedTabChangedProperty"/> from <paramref name="element"/>.
    ///
    /// If the property is set to true, the transition is triggered again when the SelectionChanged event of a TabControl was raised.
    /// </summary>
    /// <param name="element"><see cref="UIElement"/> to read <see cref="OnSelectedTabChangedProperty"/> from.</param>
    /// <returns>OnSelectedTabChanged property value.</returns>
    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(CrystalContentControl))]
    [AttachedPropertyBrowsableForType(typeof(TransitioningContentControl))]
    public static bool GetOnSelectedTabChanged(UIElement element)
    {
      return (bool)element.GetValue(OnSelectedTabChangedProperty);
    }

    /// <summary>
    /// Helper for setting <see cref="OnSelectedTabChangedProperty"/> on <paramref name="element"/>.
    ///
    /// If the property is set to true, the transition is triggered again when the SelectionChanged event of a TabControl was raised.
    /// </summary>
    /// <param name="element"><see cref="UIElement"/> to set <see cref="OnSelectedTabChangedProperty"/> on.</param>
    /// <param name="value">OnSelectedTabChanged property value.</param>
    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(CrystalContentControl))]
    [AttachedPropertyBrowsableForType(typeof(TransitioningContentControl))]
    public static void SetOnSelectedTabChanged(UIElement element, bool value)
    {
      element.SetValue(OnSelectedTabChangedProperty, BooleanBoxes.Box(value));
    }

    private static void OnSelectedTabChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue != e.NewValue)
      {
        if (dependencyObject is CrystalContentControl crystalContentControl)
        {
          crystalContentControl.Loaded -= ReloadOnLoaded;
          crystalContentControl.Loaded += ReloadOnLoaded;
        }
        else if (dependencyObject is TransitioningContentControl transitioningContentControl)
        {
          transitioningContentControl.Loaded -= ReloadOnLoaded;
          transitioningContentControl.Loaded += ReloadOnLoaded;
        }
      }
    }

    private static void ReloadOnLoaded(object sender, RoutedEventArgs e)
    {
      if (sender is ContentControl contentControl)
      {
        var tabControl = contentControl.TryFindParent<TabControl>();

        if (tabControl == null)
        {
          return;
        }

        SetContentControl(tabControl, contentControl);
        tabControl.SelectionChanged -= ReloadOnSelectionChanged;
        tabControl.SelectionChanged += ReloadOnSelectionChanged;
      }
    }

    private static void ReloadOnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (e.OriginalSource != sender)
      {
        return;
      }

      if (sender is TabControl tabControl)
      {
        var contentControl = GetContentControl(tabControl);

        if (contentControl is CrystalContentControl crystalContentControl)
        {
          crystalContentControl.Reload();
        }
        else if (contentControl is TransitioningContentControl transitioningContentControl)
        {
          transitioningContentControl.ReloadTransition();
        }
      }
    }

    internal static readonly DependencyProperty ContentControlProperty
        = DependencyProperty.RegisterAttached(
            "ContentControl",
            typeof(ContentControl),
            typeof(ReloadBehavior),
            new PropertyMetadata(default(ContentControl)));

    internal static ContentControl? GetContentControl(UIElement element)
    {
      return (ContentControl?)element.GetValue(ContentControlProperty);
    }

    internal static void SetContentControl(UIElement element, ContentControl? value)
    {
      element.SetValue(ContentControlProperty, value);
    }
  }
}