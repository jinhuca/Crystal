namespace Crystal.Themes.Behaviors;

public class DatePickerTextBoxBehavior : Behavior<DatePickerTextBox>
{
  protected override void OnAttached()
  {
    base.OnAttached();
    AssociatedObject.TextChanged += OnTextChanged;
    this.BeginInvoke(SetHasTextProperty, DispatcherPriority.Loaded);
  }

  protected override void OnDetaching()
  {
    AssociatedObject.TextChanged -= OnTextChanged;
    base.OnDetaching();
  }

  private void OnTextChanged(object sender, TextChangedEventArgs e)
  {
    SetHasTextProperty();
  }

  private void SetHasTextProperty()
  {
    AssociatedObject.TemplatedParent?.SetCurrentValue(TextBoxHelper.HasTextProperty, BooleanBoxes.Box(AssociatedObject.Text.Length > 0));
  }
}