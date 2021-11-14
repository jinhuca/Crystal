namespace Crystal.Themes.Controls
{
  /// <summary>
  /// The HamburgerMenuIconItem provides an icon based implementation for HamburgerMenu entries.
  /// </summary>
  public class HamburgerMenuIconItem : HamburgerMenuItem
  {
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
      nameof(Icon),
      typeof(object),
      typeof(HamburgerMenuIconItem),
      new PropertyMetadata(null));

    public object? Icon
    {
      get => GetValue(IconProperty);
      set => SetValue(IconProperty, value);
    }

    protected override Freezable CreateInstanceCore() => new HamburgerMenuIconItem();
  }
}