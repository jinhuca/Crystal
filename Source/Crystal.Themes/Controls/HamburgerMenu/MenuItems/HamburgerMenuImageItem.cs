namespace Crystal.Themes.Controls
{
  /// <summary>
  /// The HamburgerMenuImageItem provides an image based implementation for HamburgerMenu entries.
  /// </summary>
  public class HamburgerMenuImageItem : HamburgerMenuItem
  {
    public static readonly DependencyProperty ThumbnailProperty = DependencyProperty.Register(
      nameof(Thumbnail),
      typeof(ImageSource),
      typeof(HamburgerMenuImageItem),
      new PropertyMetadata(null));

    public ImageSource? Thumbnail
    {
      get => (ImageSource?)GetValue(ThumbnailProperty);
      set => SetValue(ThumbnailProperty, value);
    }

    protected override Freezable CreateInstanceCore() => new HamburgerMenuImageItem();
  }
}