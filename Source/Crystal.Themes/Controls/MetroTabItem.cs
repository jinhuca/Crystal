using System.Windows.Input;
using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  /// <summary>
  /// An extended TabItem with a metro style.
  /// </summary>
  public class MetroTabItem : TabItem
  {
    public MetroTabItem()
    {
      DefaultStyleKey = typeof(MetroTabItem);
    }

    public static readonly DependencyProperty CloseButtonEnabledProperty = DependencyProperty.Register(
      nameof(CloseButtonEnabled),
      typeof(bool),
      typeof(MetroTabItem),
      new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Inherits));

    public bool CloseButtonEnabled
    {
      get => (bool)GetValue(CloseButtonEnabledProperty);
      set => SetValue(CloseButtonEnabledProperty, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty CloseTabCommandProperty = DependencyProperty.Register(
      nameof(CloseTabCommand),
      typeof(ICommand),
      typeof(MetroTabItem));

    public ICommand? CloseTabCommand
    {
      get => (ICommand?)GetValue(CloseTabCommandProperty);
      set => SetValue(CloseTabCommandProperty, value);
    }

    public static readonly DependencyProperty CloseTabCommandParameterProperty = DependencyProperty.Register(
      nameof(CloseTabCommandParameter),
      typeof(object),
      typeof(MetroTabItem),
      new PropertyMetadata(null));

    public object? CloseTabCommandParameter
    {
      get => GetValue(CloseTabCommandParameterProperty);
      set => SetValue(CloseTabCommandParameterProperty, value);
    }

    public static readonly DependencyProperty CloseButtonMarginProperty = DependencyProperty.Register(
      nameof(CloseButtonMargin),
      typeof(Thickness),
      typeof(MetroTabItem),
      new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Inherits));

    public Thickness CloseButtonMargin
    {
      get => (Thickness)GetValue(CloseButtonMarginProperty);
      set => SetValue(CloseButtonMarginProperty, value);
    }
  }
}