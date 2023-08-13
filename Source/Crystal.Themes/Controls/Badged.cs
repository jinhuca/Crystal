namespace Crystal.Themes.Controls;

[TemplatePart(Name = BadgeContainerPartName, Type = typeof(UIElement))]
public class Badged : BadgedEx
{
  public static readonly DependencyProperty BadgeChangedStoryboardProperty = DependencyProperty.Register(
    nameof(BadgeChangedStoryboard),
    typeof(Storyboard),
    typeof(Badged),
    new PropertyMetadata(default(Storyboard)));

  public Storyboard? BadgeChangedStoryboard
  {
    get => (Storyboard?)GetValue(BadgeChangedStoryboardProperty);
    set => SetValue(BadgeChangedStoryboardProperty, value);
  }

  static Badged()
  {
    DefaultStyleKeyProperty.OverrideMetadata(typeof(Badged), new FrameworkPropertyMetadata(typeof(Badged)));
  }

  public override void OnApplyTemplate()
  {
    BadgeChanged -= OnBadgeChanged;
    base.OnApplyTemplate();
    BadgeChanged += OnBadgeChanged;
  }

  private void OnBadgeChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
  {
    var sb = BadgeChangedStoryboard;
    if (_badgeContainer != null && sb != null)
    {
      try
      {
        _badgeContainer.BeginStoryboard(sb);
      }
      catch (Exception exception)
      {
        throw new CrystalThemesException("Uups, it seems like there is something wrong with the given BadgeChangedStoryboard.", exception);
      }
    }
  }
}