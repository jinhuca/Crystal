namespace Crystal.Themes.Controls;

/// <summary>
/// The HamburgerMenuGlyphItem provides a glyph based implementation for HamburgerMenu entries.
/// </summary>
public class HamburgerMenuGlyphItem : HamburgerMenuItem
{
  public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
    nameof(Glyph),
    typeof(string),
    typeof(HamburgerMenuGlyphItem),
    new PropertyMetadata(null));

  public string? Glyph
  {
    get => (string?)GetValue(GlyphProperty);
    set => SetValue(GlyphProperty, value);
  }

  protected override Freezable CreateInstanceCore() => new HamburgerMenuGlyphItem();
}