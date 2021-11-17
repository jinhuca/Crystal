using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  [ContentProperty(nameof(ItemsSource))]
  [TemplatePart(Name = "PART_Container", Type = typeof(Grid))]
  [TemplatePart(Name = "PART_Button", Type = typeof(Button))]
  [TemplatePart(Name = "PART_ButtonContent", Type = typeof(ContentControl))]
  [TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
  [TemplatePart(Name = "PART_Expander", Type = typeof(Button))]
  [StyleTypedProperty(Property = nameof(ButtonStyle), StyleTargetType = typeof(Button))]
  [StyleTypedProperty(Property = nameof(ButtonArrowStyle), StyleTargetType = typeof(Button))]
  public class SplitButton : ComboBox, ICommandSource
  {
    /// <summary>Identifies the <see cref="Click"/> routed event.</summary>
    public static readonly RoutedEvent ClickEvent
        = EventManager.RegisterRoutedEvent(nameof(Click),
                                           RoutingStrategy.Bubble,
                                           typeof(RoutedEventHandler),
                                           typeof(SplitButton));

    public event RoutedEventHandler Click
    {
      add => AddHandler(ClickEvent, value);
      remove => RemoveHandler(ClickEvent, value);
    }

    /// <summary>Identifies the <see cref="ExtraTag"/> dependency property.</summary>
    public static readonly DependencyProperty ExtraTagProperty
        = DependencyProperty.Register(nameof(ExtraTag),
                                      typeof(object),
                                      typeof(SplitButton));

    /// <summary>
    /// Gets or sets an extra tag.
    /// </summary>
    public object? ExtraTag
    {
      get => GetValue(ExtraTagProperty);
      set => SetValue(ExtraTagProperty, value);
    }

    /// <summary>Identifies the <see cref="Orientation"/> dependency property.</summary>
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(nameof(Orientation),
                                      typeof(Orientation),
                                      typeof(SplitButton),
                                      new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// Gets or sets the orientation of children stacking.
    /// </summary>
    public Orientation Orientation
    {
      get => (Orientation)GetValue(OrientationProperty);
      set => SetValue(OrientationProperty, value);
    }

    /// <summary>Identifies the <see cref="Icon"/> dependency property.</summary>
    public static readonly DependencyProperty IconProperty
        = DependencyProperty.Register(nameof(Icon),
                                      typeof(object),
                                      typeof(SplitButton));

    /// <summary>
    /// Gets or sets the content for the icon part.
    /// </summary>
    [Bindable(true)]
    public object? Icon
    {
      get => GetValue(IconProperty);
      set => SetValue(IconProperty, value);
    }

    /// <summary>Identifies the <see cref="IconTemplate"/> dependency property.</summary>
    public static readonly DependencyProperty IconTemplateProperty
        = DependencyProperty.Register(nameof(IconTemplate),
                                      typeof(DataTemplate),
                                      typeof(SplitButton));

    /// <summary> 
    /// Gets or sets the DataTemplate for the icon part.
    /// </summary>
    [Bindable(true)]
    public DataTemplate? IconTemplate
    {
      get => (DataTemplate?)GetValue(IconTemplateProperty);
      set => SetValue(IconTemplateProperty, value);
    }

    /// <summary>Identifies the <see cref="Command"/> dependency property.</summary>
    public static readonly DependencyProperty CommandProperty
        = DependencyProperty.Register(nameof(Command),
                                      typeof(ICommand),
                                      typeof(SplitButton),
                                      new PropertyMetadata(null, OnCommandPropertyChangedCallback));

    private static void OnCommandPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      (dependencyObject as SplitButton)?.OnCommandChanged((ICommand?)e.OldValue, (ICommand?)e.NewValue);
    }

    /// <summary>
    /// Gets or sets the command to invoke when the content button is pressed.
    /// </summary>
    public ICommand? Command
    {
      get => (ICommand?)GetValue(CommandProperty);
      set => SetValue(CommandProperty, value);
    }

    /// <summary>Identifies the <see cref="CommandTarget"/> dependency property.</summary>
    public static readonly DependencyProperty CommandTargetProperty
        = DependencyProperty.Register(nameof(CommandTarget),
                                      typeof(IInputElement),
                                      typeof(SplitButton),
                                      new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the element on which to raise the specified command.
    /// </summary>
    public IInputElement? CommandTarget
    {
      get => (IInputElement?)GetValue(CommandTargetProperty);
      set => SetValue(CommandTargetProperty, value);
    }

    /// <summary>Identifies the <see cref="CommandParameter"/> dependency property.</summary>
    public static readonly DependencyProperty CommandParameterProperty
        = DependencyProperty.Register(nameof(CommandParameter),
                                      typeof(object),
                                      typeof(SplitButton),
                                      new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the parameter to pass to the command property.
    /// </summary>
    public object? CommandParameter
    {
      get => GetValue(CommandParameterProperty);
      set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>Identifies the <see cref="ButtonStyle"/> dependency property.</summary>
    public static readonly DependencyProperty ButtonStyleProperty
        = DependencyProperty.Register(nameof(ButtonStyle),
                                      typeof(Style),
                                      typeof(SplitButton),
                                      new FrameworkPropertyMetadata(default(Style), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// Gets or sets the button content style.
    /// </summary>
    public Style? ButtonStyle
    {
      get => (Style?)GetValue(ButtonStyleProperty);
      set => SetValue(ButtonStyleProperty, value);
    }

    /// <summary>Identifies the <see cref="ButtonArrowStyle"/> dependency property.</summary>
    public static readonly DependencyProperty ButtonArrowStyleProperty
        = DependencyProperty.Register(nameof(ButtonArrowStyle),
                                      typeof(Style),
                                      typeof(SplitButton),
                                      new FrameworkPropertyMetadata(default(Style), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// Gets or sets the button arrow style.
    /// </summary>
    public Style? ButtonArrowStyle
    {
      get => (Style?)GetValue(ButtonArrowStyleProperty);
      set => SetValue(ButtonArrowStyleProperty, value);
    }

    /// <summary>Identifies the <see cref="ArrowBrush"/> dependency property.</summary>
    public static readonly DependencyProperty ArrowBrushProperty
        = DependencyProperty.Register(nameof(ArrowBrush),
                                      typeof(Brush),
                                      typeof(SplitButton),
                                      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Gets or sets the foreground brush for the button arrow icon.
    /// </summary>
    public Brush? ArrowBrush
    {
      get => (Brush?)GetValue(ArrowBrushProperty);
      set => SetValue(ArrowBrushProperty, value);
    }

    /// <summary>Identifies the <see cref="ArrowMouseOverBrush"/> dependency property.</summary>
    public static readonly DependencyProperty ArrowMouseOverBrushProperty
        = DependencyProperty.Register(nameof(ArrowMouseOverBrush),
                                      typeof(Brush),
                                      typeof(SplitButton),
                                      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Gets or sets the foreground brush of the button arrow icon if the mouse is over the split button.
    /// </summary>
    public Brush? ArrowMouseOverBrush
    {
      get => (Brush?)GetValue(ArrowMouseOverBrushProperty);
      set => SetValue(ArrowMouseOverBrushProperty, value);
    }

    /// <summary>Identifies the <see cref="ArrowPressedBrush"/> dependency property.</summary>
    public static readonly DependencyProperty ArrowPressedBrushProperty
        = DependencyProperty.Register(nameof(ArrowPressedBrush),
                                      typeof(Brush),
                                      typeof(SplitButton),
                                      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Gets or sets the foreground brush of the button arrow icon if the arrow button is pressed.
    /// </summary>
    public Brush? ArrowPressedBrush
    {
      get => (Brush?)GetValue(ArrowPressedBrushProperty);
      set => SetValue(ArrowPressedBrushProperty, value);
    }

    static SplitButton()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(typeof(SplitButton)));

      IsEditableProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, null, CoerceIsEditableProperty));
    }

    private void OnCommandChanged(ICommand? oldCommand, ICommand? newCommand)
    {
      if (oldCommand != null)
      {
        UnhookCommand(oldCommand);
      }

      if (newCommand != null)
      {
        HookCommand(newCommand);
      }
    }

    private void UnhookCommand(ICommand command)
    {
      CanExecuteChangedEventManager.RemoveHandler(command, OnCanExecuteChanged);
      UpdateCanExecute();
    }

    private void HookCommand(ICommand command)
    {
      CanExecuteChangedEventManager.AddHandler(command, OnCanExecuteChanged);
      UpdateCanExecute();
    }

    private void OnCanExecuteChanged(object? sender, EventArgs e)
    {
      UpdateCanExecute();
    }

    private void UpdateCanExecute()
    {
      CanExecute = Command == null || CommandHelpers.CanExecuteCommandSource(this);
    }

    /// <inheritdoc />
    protected override bool IsEnabledCore => base.IsEnabledCore && CanExecute;

    private bool canExecute = true;

    private bool CanExecute
    {
      get => canExecute;
      set
      {
        if (value == canExecute)
        {
          return;
        }

        canExecute = value;
        CoerceValue(IsEnabledProperty);
      }
    }

    [MustUseReturnValue]
    private static object CoerceIsEditableProperty(DependencyObject dependencyObject, object? value)
    {
      // For now SplitButton is not editable
      return false;
    }

    private void ButtonClick(object sender, RoutedEventArgs e)
    {
      CommandHelpers.ExecuteCommandSource(this);

      e.RoutedEvent = ClickEvent;
      RaiseEvent(e);

      SetCurrentValue(IsDropDownOpenProperty, BooleanBoxes.FalseBox);
    }

    private void ExpanderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      SetCurrentValue(IsDropDownOpenProperty, BooleanBoxes.Box(!IsDropDownOpen));
      e.Handled = true;
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if (button != null)
      {
        button.Click -= ButtonClick;
      }

      button = GetTemplateChild("PART_Button") as Button;
      if (button != null)
      {
        button.Click += ButtonClick;
      }

      if (expanderButton != null)
      {
        expanderButton.PreviewMouseLeftButtonDown -= ExpanderMouseLeftButtonDown;
      }

      expanderButton = GetTemplateChild("PART_Expander") as Button;
      if (expanderButton != null)
      {
        expanderButton.PreviewMouseLeftButtonDown += ExpanderMouseLeftButtonDown;
      }
    }

    private Button? button;
    private Button? expanderButton;
  }
}