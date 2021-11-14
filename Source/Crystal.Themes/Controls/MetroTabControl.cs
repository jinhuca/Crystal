using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  /// <summary>
  /// A standard MetroTabControl (Pivot).
  /// </summary>
  public class MetroTabControl : BaseMetroTabControl
  {
    static MetroTabControl()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(MetroTabControl), new FrameworkPropertyMetadata(typeof(MetroTabControl)));
    }

    public static readonly DependencyProperty KeepVisualTreeInMemoryWhenChangingTabsProperty = DependencyProperty.Register(
      nameof(KeepVisualTreeInMemoryWhenChangingTabs),
      typeof(bool),
      typeof(MetroTabControl),
      new PropertyMetadata(BooleanBoxes.FalseBox));

    public bool KeepVisualTreeInMemoryWhenChangingTabs
    {
      get => (bool)GetValue(KeepVisualTreeInMemoryWhenChangingTabsProperty);
      set => SetValue(KeepVisualTreeInMemoryWhenChangingTabsProperty, BooleanBoxes.Box(value));
    }
  }
}