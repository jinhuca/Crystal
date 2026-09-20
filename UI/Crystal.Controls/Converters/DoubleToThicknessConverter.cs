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
  /// <summary>
  /// Converts a <see cref="double"/> value to a <see cref="Thickness"/> with uniform thickness.
  /// </summary>
  /// <param name="value">The double value to convert.</param>
  /// <param name="targetType">The type of the target.</param>
  /// <param name="parameter">The converter parameter.</param>
  /// <param name="culture">The culture to use.</param>
  /// <returns>The converted Thickness value.</returns>
  public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    => value is double d ? new Thickness(d) : new Thickness(0);

  /// <summary>
  /// Converts a <see cref="Thickness"/> back to a <see cref="double"/> by returning the left thickness value.
  /// </summary>
  /// <param name="value">The Thickness value to convert.</param>
  /// <param name="targetType">The type of the target.</param>
  /// <param name="parameter">The converter parameter.</param>
  /// <param name="culture">The culture to use.</param>
  /// <returns>The converted double value.</returns>
  public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
    => value is Thickness t ? t.Left : 0d;
}
