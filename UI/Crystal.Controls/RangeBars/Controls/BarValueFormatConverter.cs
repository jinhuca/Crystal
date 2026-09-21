using System.Globalization;
using System.Windows.Data;

namespace Crystal.Controls.RangeBars.Controls;

/// <summary>
/// Formats a numeric value with a composite format string supplied as the second binding,
/// e.g. value 0.82 + "{0:0.00}" => "0.82". Lets the label format itself be a bindable property
/// rather than a static XAML <c>StringFormat</c> (which cannot be data-bound).
/// </summary>
[ValueConversion(typeof(double), typeof(string))]
public sealed class BarValueFormatConverter : IMultiValueConverter {
  /// <summary>
  /// Converts the specified values using the provided format string.
  /// </summary>
  /// <param name="values">The values to convert.</param>
  /// <param name="targetType">The type of the target.</param>
  /// <param name="parameter">The parameter to use for conversion.</param>
  /// <param name="culture">The culture to use for conversion.</param>
  /// <returns>The converted value.</returns>
  public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
    if (values is null || values.Length < 2) return string.Empty;
    object value = values[0];
    string format = values[1] as string ?? "{0}";
    try {
      return string.Format(culture, format, value);
    }
    catch (FormatException) {
      return value?.ToString() ?? string.Empty;
    }
  }

  /// <summary>
  /// Converts a value back to its original form. This method is not supported and will throw a NotSupportedException.
  /// </summary>
  /// <param name="value">The value to convert back.</param>
  /// <param name="targetTypes">The types of the targets.</param>
  /// <param name="parameter">The parameter to use for conversion.</param>
  /// <param name="culture">The culture to use for conversion.</param>
  /// <returns>The converted value.</returns>
  /// <exception cref="NotSupportedException"></exception>
  /// <returns></returns>
  /// <exception cref="NotSupportedException"></exception>
  public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
    throw new NotSupportedException();
}
