namespace Crystal.Themes.Controls;

/// <summary>
/// A standard CrystalTabControl (Pivot).
/// </summary>
public class CrystalTabControl : CrystalTabControlBase
{
  static CrystalTabControl()
  {
    DefaultStyleKeyProperty.OverrideMetadata(typeof(CrystalTabControl), new FrameworkPropertyMetadata(typeof(CrystalTabControl)));
  }

  public static readonly DependencyProperty KeepVisualTreeInMemoryWhenChangingTabsProperty = DependencyProperty.Register(
    nameof(KeepVisualTreeInMemoryWhenChangingTabs),
    typeof(bool),
    typeof(CrystalTabControl),
    new PropertyMetadata(BooleanBoxes.FalseBox));

  public bool KeepVisualTreeInMemoryWhenChangingTabs
  {
    get => (bool)GetValue(KeepVisualTreeInMemoryWhenChangingTabsProperty);
    set => SetValue(KeepVisualTreeInMemoryWhenChangingTabsProperty, BooleanBoxes.Box(value));
  }
}