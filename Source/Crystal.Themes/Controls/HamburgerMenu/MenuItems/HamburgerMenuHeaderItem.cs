namespace Crystal.Themes.Controls;

public class HamburgerMenuHeaderItem : HamburgerMenuItemBase, IHamburgerMenuHeaderItem
{
  public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
    nameof(Label),
    typeof(string),
    typeof(HamburgerMenuHeaderItem),
    new PropertyMetadata(null));

  public string? Label
  {
    get => (string?)GetValue(LabelProperty);
    set => SetValue(LabelProperty, value);
  }

  protected override Freezable CreateInstanceCore() => new HamburgerMenuHeaderItem();
}