using System.Windows.Automation;

namespace Crystal.Themes.Controls
{
  [StyleTypedProperty(Property = nameof(HamburgerButtonStyle), StyleTargetType = typeof(Button))]
  public partial class HamburgerMenu
  {
    public static readonly DependencyProperty HamburgerWidthProperty = DependencyProperty.Register(
      nameof(HamburgerWidth),
      typeof(double),
      typeof(HamburgerMenu),
      new PropertyMetadata(48.0));

    public double HamburgerWidth
    {
      get => (double)GetValue(HamburgerWidthProperty);
      set => SetValue(HamburgerWidthProperty, value);
    }

    public static readonly DependencyProperty HamburgerHeightProperty = DependencyProperty.Register(
      nameof(HamburgerHeight),
      typeof(double),
      typeof(HamburgerMenu),
      new PropertyMetadata(48.0));

    public double HamburgerHeight
    {
      get => (double)GetValue(HamburgerHeightProperty);
      set => SetValue(HamburgerHeightProperty, value);
    }

    public static readonly DependencyProperty HamburgerMarginProperty = DependencyProperty.Register(
      nameof(HamburgerMargin),
      typeof(Thickness),
      typeof(HamburgerMenu),
      new PropertyMetadata(new Thickness()));

    public Thickness HamburgerMargin
    {
      get => (Thickness)GetValue(HamburgerMarginProperty);
      set => SetValue(HamburgerMarginProperty, value);
    }

    public static readonly DependencyProperty HamburgerVisibilityProperty = DependencyProperty.Register(
      nameof(HamburgerVisibility),
      typeof(Visibility),
      typeof(HamburgerMenu),
      new PropertyMetadata(Visibility.Visible));

    public Visibility HamburgerVisibility
    {
      get => (Visibility)GetValue(HamburgerVisibilityProperty);
      set => SetValue(HamburgerVisibilityProperty, value);
    }

    public static readonly DependencyProperty HamburgerButtonStyleProperty = DependencyProperty.Register(
      nameof(HamburgerButtonStyle),
      typeof(Style),
      typeof(HamburgerMenu),
      new PropertyMetadata(null));

    public Style? HamburgerButtonStyle
    {
      get => (Style?)GetValue(HamburgerButtonStyleProperty);
      set => SetValue(HamburgerButtonStyleProperty, value);
    }

    public static readonly DependencyProperty HamburgerButtonTemplateProperty = DependencyProperty.Register(
      nameof(HamburgerButtonTemplate),
      typeof(DataTemplate),
      typeof(HamburgerMenu),
      new PropertyMetadata(null));

    public DataTemplate? HamburgerButtonTemplate
    {
      get => (DataTemplate?)GetValue(HamburgerButtonTemplateProperty);
      set => SetValue(HamburgerButtonTemplateProperty, value);
    }

    public static readonly DependencyProperty HamburgerButtonNameProperty = DependencyProperty.Register(
      nameof(HamburgerButtonName),
      typeof(string),
      typeof(HamburgerMenu),
      new UIPropertyMetadata(string.Empty), new ValidateValueCallback(IsNotNull));

    public string HamburgerButtonName
    {
      get => (string)GetValue(HamburgerButtonNameProperty);
      set => SetValue(HamburgerButtonNameProperty, value);
    }

    public static readonly DependencyProperty HamburgerButtonHelpTextProperty = DependencyProperty.Register(
      nameof(HamburgerButtonHelpText),
      typeof(string),
      typeof(HamburgerMenu),
      new UIPropertyMetadata(string.Empty), new ValidateValueCallback(IsNotNull));

    public string HamburgerButtonHelpText
    {
      get => (string)GetValue(HamburgerButtonHelpTextProperty);
      set => SetValue(HamburgerButtonHelpTextProperty, value);
    }

    public static readonly DependencyProperty HamburgerMenuHeaderTemplateProperty = DependencyProperty.Register(
      nameof(HamburgerMenuHeaderTemplate),
      typeof(DataTemplate),
      typeof(HamburgerMenu),
      new PropertyMetadata(null));

    public DataTemplate? HamburgerMenuHeaderTemplate
    {
      get => (DataTemplate?)GetValue(HamburgerMenuHeaderTemplateProperty);
      set => SetValue(HamburgerMenuHeaderTemplateProperty, value);
    }

    private static bool IsNotNull(object? value) => value != null;
  }
}