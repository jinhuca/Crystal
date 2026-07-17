using System;
using System.Globalization;
using System.Windows.Data;

namespace ResourceModule.Converters;

public class DoublePartConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (value is double num) {
      // Get the current culture's decimal separator
      string separator = culture.NumberFormat.NumberDecimalSeparator;

      // Format the number to a standard string
      string[] parts = num.ToString("F2", culture).Split(separator);

      // parameter "int" returns whole number, "dec" returns decimal part
      if (parameter?.ToString() == "dec") {
        return separator + parts[1];
      }
      return parts[0];
    }
    return string.Empty;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    throw new NotImplementedException();
  }
}
