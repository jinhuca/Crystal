using System.Globalization;
using System.Windows.Data;

namespace Crystal.Controls.PerformanceGraphs.Controls;

/// <summary>
/// Formats a numeric value with a composite format string supplied as the second binding,
/// e.g. value 100 + "{0}%" => "100%". Lets the label format itself be a bindable property
/// rather than a static XAML <c>StringFormat</c> (which cannot be data-bound).
/// </summary>
[ValueConversion(typeof(object), typeof(string))]
public sealed class GraphValueFormatConverter : IMultiValueConverter {
  /// <summary>
  /// Converts a numeric value and a composite format string into a formatted string. 
  /// The first value is the numeric value, and the second value is the format string. 
  /// If the format string is null or invalid, returns the numeric value as a string.
  /// </summary>
  /// <param name="values">The values to convert.</param>
  /// <param name="targetType">The type of the target.</param>
  /// <param name="parameter">The parameter for the converter.</param>
  /// <param name="culture">The culture to use for formatting.</param>
  /// <returns>The formatted string.</returns>
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
  /// Not supported. This converter does not support converting back from a formatted string to the original values.
  /// </summary>
  /// <param name="value">The value to convert back.</param>
  /// <param name="targetTypes">The types of the targets.</param>
  /// <param name="parameter">The parameter for the converter.</param>
  /// <param name="culture">The culture to use for formatting.</param>
  /// <returns></returns>
  /// <exception cref="NotSupportedException"></exception>
  public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
    throw new NotSupportedException();
}
