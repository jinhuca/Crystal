using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Crystal.ProcessModule.Views;

/// <summary>
/// Maps null/non-null to <see cref="Visibility"/>. By default a non-null value is Visible; pass
/// the parameter "invert" to flip it (non-null → Collapsed), which drives the "nothing selected"
/// hint in the detail panel.
/// </summary>
public sealed class NullToVisibilityConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    bool hasValue = value is not null;
    bool invert = string.Equals(parameter as string, "invert", StringComparison.OrdinalIgnoreCase);
    return (hasValue ^ invert) ? Visibility.Visible : Visibility.Collapsed;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}
