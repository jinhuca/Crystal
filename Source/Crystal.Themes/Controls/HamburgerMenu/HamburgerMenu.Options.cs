namespace Crystal.Themes.Controls;

[StyleTypedProperty(Property = nameof(OptionsItemContainerStyle), StyleTargetType = typeof(ListBoxItem))]
public partial class HamburgerMenu
{
  public static readonly DependencyProperty OptionsItemsSourceProperty = DependencyProperty.Register(
    nameof(OptionsItemsSource),
    typeof(object),
    typeof(HamburgerMenu),
    new PropertyMetadata(null));

  public object? OptionsItemsSource
  {
    get => GetValue(OptionsItemsSourceProperty);
    set => SetValue(OptionsItemsSourceProperty, value);
  }

  public static readonly DependencyProperty OptionsItemContainerStyleProperty = DependencyProperty.Register(
    nameof(OptionsItemContainerStyle),
    typeof(Style),
    typeof(HamburgerMenu),
    new PropertyMetadata(null));

  public Style? OptionsItemContainerStyle
  {
    get => (Style?)GetValue(OptionsItemContainerStyleProperty);
    set => SetValue(OptionsItemContainerStyleProperty, value);
  }

  public static readonly DependencyProperty OptionsItemTemplateProperty = DependencyProperty.Register(
    nameof(OptionsItemTemplate),
    typeof(DataTemplate),
    typeof(HamburgerMenu),
    new PropertyMetadata(null));

  public DataTemplate? OptionsItemTemplate
  {
    get => (DataTemplate?)GetValue(OptionsItemTemplateProperty);
    set => SetValue(OptionsItemTemplateProperty, value);
  }

  public static readonly DependencyProperty OptionsItemTemplateSelectorProperty = DependencyProperty.Register(
    nameof(OptionsItemTemplateSelector),
    typeof(DataTemplateSelector),
    typeof(HamburgerMenu),
    new PropertyMetadata(null));

  public DataTemplateSelector? OptionsItemTemplateSelector
  {
    get => (DataTemplateSelector?)GetValue(OptionsItemTemplateSelectorProperty);
    set => SetValue(OptionsItemTemplateSelectorProperty, value);
  }

  public static readonly DependencyProperty OptionsVisibilityProperty = DependencyProperty.Register(
    nameof(OptionsVisibility),
    typeof(Visibility),
    typeof(HamburgerMenu),
    new PropertyMetadata(Visibility.Visible));

  public Visibility OptionsVisibility
  {
    get => (Visibility)GetValue(OptionsVisibilityProperty);
    set => SetValue(OptionsVisibilityProperty, value);
  }

  public static readonly DependencyProperty SelectedOptionsItemProperty = DependencyProperty.Register(
    nameof(SelectedOptionsItem),
    typeof(object),
    typeof(HamburgerMenu),
    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

  public object? SelectedOptionsItem
  {
    get => GetValue(SelectedOptionsItemProperty);
    set => SetValue(SelectedOptionsItemProperty, value);
  }

  public static readonly DependencyProperty SelectedOptionsIndexProperty = DependencyProperty.Register(
    nameof(SelectedOptionsIndex),
    typeof(int),
    typeof(HamburgerMenu),
    new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal));

  public int SelectedOptionsIndex
  {
    get => (int)GetValue(SelectedOptionsIndexProperty);
    set => SetValue(SelectedOptionsIndexProperty, value);
  }

  public static readonly DependencyProperty OptionsItemCommandProperty = DependencyProperty.Register(
    nameof(OptionsItemCommand),
    typeof(ICommand),
    typeof(HamburgerMenu),
    new PropertyMetadata(null));

  public ICommand? OptionsItemCommand
  {
    get => (ICommand?)GetValue(OptionsItemCommandProperty);
    set => SetValue(OptionsItemCommandProperty, value);
  }

  public static readonly DependencyProperty OptionsItemCommandParameterProperty = DependencyProperty.Register(
    nameof(OptionsItemCommandParameter),
    typeof(object),
    typeof(HamburgerMenu),
    new PropertyMetadata(null));

  public object? OptionsItemCommandParameter
  {
    get => GetValue(OptionsItemCommandParameterProperty);
    set => SetValue(OptionsItemCommandParameterProperty, value);
  }

  public ItemCollection OptionsItems
  {
    get
    {
      if (optionsListView is null)
      {
        throw new Exception("OptionsListView is not defined yet. Please use OptionsItemsSource instead.");
      }

      return optionsListView.Items;
    }
  }

  public void RaiseOptionsItemCommand()
  {
    var command = OptionsItemCommand;
    var commandParameter = OptionsItemCommandParameter ?? this;
    if (command != null && command.CanExecute(commandParameter))
    {
      command.Execute(commandParameter);
    }
  }
}