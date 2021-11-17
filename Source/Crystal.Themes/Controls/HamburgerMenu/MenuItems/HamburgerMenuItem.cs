using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  /// <summary>
  /// The HamburgerMenuItem provides an implementation for HamburgerMenu entries.
  /// </summary>
  public class HamburgerMenuItem : HamburgerMenuItemBase, IHamburgerMenuItem, ICommandSource
  {
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
      nameof(Label),
      typeof(string),
      typeof(HamburgerMenuItem),
      new PropertyMetadata(null));

    public string? Label
    {
      get => (string?)GetValue(LabelProperty);
      set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty TargetPageTypeProperty = DependencyProperty.Register(
      nameof(TargetPageType),
      typeof(Type),
      typeof(HamburgerMenuItem),
      new PropertyMetadata(null));

    public Type? TargetPageType
    {
      get => (Type?)GetValue(TargetPageTypeProperty);
      set => SetValue(TargetPageTypeProperty, value);
    }

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
      nameof(Command),
      typeof(ICommand),
      typeof(HamburgerMenuItem),
      new PropertyMetadata(null, OnCommandPropertyChanged));

    private static void OnCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((HamburgerMenuItem)d).OnCommandChanged(e.OldValue as ICommand, e.NewValue as ICommand);
    }

    public ICommand? Command
    {
      get => (ICommand?)GetValue(CommandProperty);
      set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
      nameof(CommandParameter),
      typeof(object),
      typeof(HamburgerMenuItem),
      new PropertyMetadata(null));

    public object? CommandParameter
    {
      get => GetValue(CommandParameterProperty);
      set => SetValue(CommandParameterProperty, value);
    }

    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
      nameof(CommandTarget),
      typeof(IInputElement),
      typeof(HamburgerMenuItem),
      new PropertyMetadata(null));

    public IInputElement? CommandTarget
    {
      get => (IInputElement?)GetValue(CommandTargetProperty);
      set => SetValue(CommandTargetProperty, value);
    }

    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
      nameof(IsEnabled),
      typeof(bool),
      typeof(HamburgerMenuItem),
      new PropertyMetadata(BooleanBoxes.TrueBox, null, IsEnabledCoerceValueCallback));

    [MustUseReturnValue]
    private static object IsEnabledCoerceValueCallback(DependencyObject d, object? value)
    {
      if (value is bool isEnabled && isEnabled == false)
      {
        return BooleanBoxes.FalseBox;
      }

      return ((HamburgerMenuItem)d).CanExecute;
    }

    public bool IsEnabled
    {
      get => (bool)GetValue(IsEnabledProperty);
      set => SetValue(IsEnabledProperty, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty ToolTipProperty = DependencyProperty.Register(
      nameof(ToolTip),
      typeof(object),
      typeof(HamburgerMenuItem),
      new PropertyMetadata(null));

    public object? ToolTip
    {
      get => GetValue(ToolTipProperty);
      set => SetValue(ToolTipProperty, value);
    }

    public void RaiseCommand()
    {
      CommandHelpers.ExecuteCommandSource(this);
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
      CanExecute = Command is null || CommandHelpers.CanExecuteCommandSource(this);
    }

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

    protected override Freezable CreateInstanceCore()
    {
      return new HamburgerMenuItem();
    }
  }
}