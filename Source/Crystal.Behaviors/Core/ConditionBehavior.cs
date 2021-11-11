using System.Windows.Markup;

namespace Crystal.Behaviors
{
  /// <summary>
  /// A behavior that attaches to a trigger and controls the conditions
  /// to fire the actions. 
  /// </summary>
  /// 
  [ContentProperty("Condition")]
  public class ConditionBehavior : Behavior<TriggerBase>
  {
    public static readonly System.Windows.DependencyProperty ConditionProperty = System.Windows.DependencyProperty.Register(
      nameof(Condition),
      typeof(ICondition),
      typeof(ConditionBehavior),
      new System.Windows.PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the IConditon object on behavior.
    /// </summary>
    /// <value>The name of the condition to change.</value>
    public ICondition Condition
    {
      get => (ICondition)GetValue(ConditionProperty);
      init => SetValue(ConditionProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionBehavior"/> class.
    /// </summary>
    public ConditionBehavior()
    {
    }

    protected override void OnAttached()
    {
      base.OnAttached();
      AssociatedObject.PreviewInvoke += OnPreviewInvoke;
    }

    protected override void OnDetaching()
    {
      AssociatedObject.PreviewInvoke -= OnPreviewInvoke;
      base.OnDetaching();
    }

    /// <summary>
    /// The event handler that is listening to the preview invoke event that is fired by 
    /// the trigger. Setting PreviewInvokeEventArgs.Cancelling to True will
    /// cancel the invocation.
    /// </summary>
    /// <param name="sender">The trigger base object.</param>
    /// <param name="e">An object of type PreviewInvokeEventArgs where e.Cancelling can be set to True.</param>
    private void OnPreviewInvoke(object sender, PreviewInvokeEventArgs e)
    {
      if (Condition != null)
      {
        e.Cancelling = !Condition.Evaluate();
      }
    }
  }
}
