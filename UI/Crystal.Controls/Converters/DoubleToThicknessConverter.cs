using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Crystal.Controls.Converters;

/// <summary>
/// Wraps a uniform <see cref="double"/> into a <see cref="Thickness"/> so a single
/// scalar (e.g. a slider value) can drive a BorderThickness.
/// </summary>
[ValueConversion(typeof(double), typeof(Thickness))]
public sealed class DoubleToThicknessConverter : IValueConverter {
  public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
      => value is double d ? new Thickness(d) : new Thickness(0);

  public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
      => value is Thickness t ? t.Left : 0d;
}
