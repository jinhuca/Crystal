using Crystal.Behaviors;
using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Actions
{
  /// <summary>
  /// This CommandTriggerAction can be used to bind any event on any FrameworkElement to an <see cref="ICommand"/>.
  /// This trigger can only be attached to a FrameworkElement or a class deriving from FrameworkElement.
  /// 
  /// This class is inspired from Laurent Bugnion and his EventToCommand.
  /// <web>http://www.mvvmlight.net</web>
  /// <license> See license.txt in this solution or http://www.galasoft.ch/license_MIT.txt </license>
  /// </summary>
  public class CommandTriggerAction : TriggerAction<FrameworkElement>
  {
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
      nameof(Command),
      typeof(ICommand),
      typeof(CommandTriggerAction),
      new PropertyMetadata(null, (s, e) => OnCommandChanged(s as CommandTriggerAction, e)));

    public ICommand? Command
    {
      get => (ICommand?)GetValue(CommandProperty);
      set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
      nameof(CommandParameter),
      typeof(object),
      typeof(CommandTriggerAction),
      new PropertyMetadata(null, (s, e) =>
      {
        var sender = s as CommandTriggerAction;
        if (sender?.AssociatedObject != null)
        {
          sender.EnableDisableElement();
        }
      }));

    public object? CommandParameter
    {
      get => GetValue(CommandParameterProperty);
      set => SetValue(CommandParameterProperty, value);
    }

    public bool PassAssociatedObjectToCommand { get; set; }

    public CommandTriggerAction() => PassAssociatedObjectToCommand = true;

    protected override void OnAttached()
    {
      base.OnAttached();
      EnableDisableElement();
    }

    protected override void Invoke(object parameter)
    {
      if (AssociatedObject is null || (AssociatedObject != null && !AssociatedObject.IsEnabled))
      {
        return;
      }

      var command = Command;
      if (command != null)
      {
        var commandParameter = GetCommandParameter();
        if (command.CanExecute(commandParameter))
        {
          command.Execute(commandParameter);
        }
      }
    }

    private static void OnCommandChanged(CommandTriggerAction? action, DependencyPropertyChangedEventArgs e)
    {
      if (action is null)
      {
        return;
      }

      if (e.OldValue is ICommand oldCommand)
      {
        oldCommand.CanExecuteChanged -= action.OnCommandCanExecuteChanged;
      }

      if (e.NewValue is ICommand newCommand)
      {
        newCommand.CanExecuteChanged += action.OnCommandCanExecuteChanged;
      }

      action.EnableDisableElement();
    }

    protected virtual object? GetCommandParameter()
    {
      var parameter = CommandParameter;
      if (parameter is null && PassAssociatedObjectToCommand)
      {
        parameter = AssociatedObject;
      }

      return parameter;
    }

    private void EnableDisableElement()
    {
      if (AssociatedObject is null)
      {
        return;
      }

      var command = Command;
      AssociatedObject.SetCurrentValue(UIElement.IsEnabledProperty, BooleanBoxes.Box(command is null || command.CanExecute(GetCommandParameter())));
    }

    private void OnCommandCanExecuteChanged(object? sender, EventArgs e)
    {
      EnableDisableElement();
    }
  }
}