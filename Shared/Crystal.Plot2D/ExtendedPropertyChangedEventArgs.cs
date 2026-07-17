using System;
using System.Windows;

namespace Crystal.Plot2D;

public sealed class ExtendedPropertyChangedEventArgs : EventArgs {
  internal string PropertyName { get; set; }
  internal object OldValue { get; set; }
  internal object NewValue { get; set; }

  internal static ExtendedPropertyChangedEventArgs FromDependencyPropertyChanged(DependencyPropertyChangedEventArgs e)
    => new() { PropertyName = e.Property.Name, NewValue = e.NewValue, OldValue = e.OldValue };
}
