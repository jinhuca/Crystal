using System;
using System.Globalization;
using System.Windows.Data;

namespace Crystal.Controls.RangeBars.Controls;

/// <summary>
/// Formats a numeric value with a composite format string supplied as the second binding,
/// e.g. value 0.82 + "{0:0.00}" => "0.82". Lets the label format itself be a bindable property
/// rather than a static XAML <c>StringFormat</c> (which cannot be data-bound).
/// </summary>
public sealed class BarValueFormatConverter : IMultiValueConverter {
  public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
    if (values is null || values.Length < 2) return string.Empty;
    object value = values[0];
    string format = values[1] as string ?? "{0}";
    try {
      return string.Format(culture, format, value);
    } catch (FormatException) {
      return value?.ToString() ?? string.Empty;
    }
  }

  public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}
