using System;
using System.Globalization;
using System.Windows.Data;

namespace ProcessModule.Views;

/// <summary>
/// Formats a nullable bytes-per-second rate as a compact human string (e.g. "1.2 MB/s").
/// A null rate renders as an em-dash — the metric isn't wired yet, which is distinct from a
/// real zero. Values are decimal (÷1000) to match Task Manager's disk/network readouts.
/// </summary>
public sealed class ByteRateConverter : IValueConverter {
  private static readonly string[] Units = { "B/s", "KB/s", "MB/s", "GB/s" };

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (value is not double rate) return "—";

    double v = rate;
    int unit = 0;
    while (v >= 1000 && unit < Units.Length - 1) {
      v /= 1000;
      unit++;
    }
    // Whole bytes need no decimal; larger units read better with one.
    string number = unit == 0 ? v.ToString("0", culture) : v.ToString("0.0", culture);
    return $"{number} {Units[unit]}";
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}
