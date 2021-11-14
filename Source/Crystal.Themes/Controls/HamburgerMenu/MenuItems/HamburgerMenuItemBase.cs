using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  public class HamburgerMenuItemBase : Freezable, IHamburgerMenuItemBase
  {
    public static readonly DependencyProperty TagProperty = DependencyProperty.Register(
      nameof(Tag),
      typeof(object),
      typeof(HamburgerMenuItemBase),
      new PropertyMetadata(null));

    public object? Tag
    {
      get => GetValue(TagProperty);
      set => SetValue(TagProperty, value);
    }

    public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.Register(
      nameof(IsVisible),
      typeof(bool),
      typeof(HamburgerMenuItemBase),
      new PropertyMetadata(BooleanBoxes.TrueBox));

    public bool IsVisible
    {
      get => (bool)GetValue(IsVisibleProperty);
      set => SetValue(IsVisibleProperty, BooleanBoxes.Box(value));
    }

    protected override Freezable CreateInstanceCore() => new HamburgerMenuItemBase();
  }
}