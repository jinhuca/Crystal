using System.Windows.Input;
using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  /// <summary>
  /// An extended TabItem with a crystal style.
  /// </summary>
  public class CrystalTabItem : TabItem
  {
    public CrystalTabItem()
    {
      DefaultStyleKey = typeof(CrystalTabItem);
    }

    public static readonly DependencyProperty CloseButtonEnabledProperty = DependencyProperty.Register(
      nameof(CloseButtonEnabled),
      typeof(bool),
      typeof(CrystalTabItem),
      new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Inherits));

    public bool CloseButtonEnabled
    {
      get => (bool)GetValue(CloseButtonEnabledProperty);
      set => SetValue(CloseButtonEnabledProperty, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty CloseTabCommandProperty = DependencyProperty.Register(
      nameof(CloseTabCommand),
      typeof(ICommand),
      typeof(CrystalTabItem));

    public ICommand? CloseTabCommand
    {
      get => (ICommand?)GetValue(CloseTabCommandProperty);
      set => SetValue(CloseTabCommandProperty, value);
    }

    public static readonly DependencyProperty CloseTabCommandParameterProperty = DependencyProperty.Register(
      nameof(CloseTabCommandParameter),
      typeof(object),
      typeof(CrystalTabItem),
      new PropertyMetadata(null));

    public object? CloseTabCommandParameter
    {
      get => GetValue(CloseTabCommandParameterProperty);
      set => SetValue(CloseTabCommandParameterProperty, value);
    }

    public static readonly DependencyProperty CloseButtonMarginProperty = DependencyProperty.Register(
      nameof(CloseButtonMargin),
      typeof(Thickness),
      typeof(CrystalTabItem),
      new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Inherits));

    public Thickness CloseButtonMargin
    {
      get => (Thickness)GetValue(CloseButtonMarginProperty);
      set => SetValue(CloseButtonMarginProperty, value);
    }
  }
}