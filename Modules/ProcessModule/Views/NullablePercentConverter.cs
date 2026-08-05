using System;
using System.Globalization;
using System.Windows.Data;

namespace ProcessModule.Views;

/// <summary>
/// Formats a nullable percent as "12.3%", or an em-dash when null so an unwired metric reads as
/// "no data" rather than a misleading 0.0%.
/// </summary>
public sealed class NullablePercentConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      value is double p ? p.ToString("0.0", culture) + "%" : "—";

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}
