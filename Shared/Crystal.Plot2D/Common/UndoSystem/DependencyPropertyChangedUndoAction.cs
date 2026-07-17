using System.Windows;

namespace Crystal.Plot2D.Common.UndoSystem;

public sealed class DependencyPropertyChangedUndoAction : UndoAction {
  private DependencyProperty Property { get; }
  private DependencyObject Target { get; }
  private object OldValue { get; }
  private object NewValue { get; }

  public DependencyPropertyChangedUndoAction(DependencyObject target, DependencyProperty property, object oldValue, object newValue) {
    Target = target;
    Property = property;
    OldValue = oldValue;
    NewValue = newValue;
  }

  public DependencyPropertyChangedUndoAction(DependencyObject target, DependencyPropertyChangedEventArgs e) {
    Target = target;
    Property = e.Property;
    OldValue = e.OldValue;
    NewValue = e.NewValue;
  }

  public override void Do() => Target.SetValue(dp: Property, value: NewValue);

  public override void Undo() => Target.SetValue(dp: Property, value: OldValue);
}
