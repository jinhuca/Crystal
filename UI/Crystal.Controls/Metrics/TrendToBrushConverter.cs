using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Crystal.Controls.Metrics;

/// <summary>
/// Maps a <see cref="MetricTrend"/> to a frozen brush: warm for rising, cool for falling, muted for
/// steady. Direction-only semantics (no good/bad judgement), since "up" is desirable for a clock but
/// not for a temperature — the color reads as motion, not severity.
/// </summary>
[ValueConversion(typeof(MetricTrend), typeof(Brush))]
public sealed class TrendToBrushConverter : IValueConverter {
  /// <summary>
  /// Maps a <see cref="MetricTrend"/> to a frozen brush: warm for rising, cool for falling, muted for
  /// steady. Direction-only semantics (no good/bad judgement), since "up" is desirable for a clock but
  /// not for a temperature — the color reads as motion, not severity.
  /// </summary>
  private static readonly SolidColorBrush Rising = Freeze(0xE8, 0xB3, 0x2A);

  /// <summary>
  /// Maps a <see cref="MetricTrend"/> to a frozen brush: warm for rising, cool for falling, muted for
  /// steady. Direction-only semantics (no good/bad judgement), since "up" is desirable for a clock but
  /// not for a temperature — the color reads as motion, not severity.
  /// </summary>
  private static readonly SolidColorBrush Falling = Freeze(0x4A, 0xA3, 0xE8);

  /// <summary>
  /// Maps a <see cref="MetricTrend"/> to a frozen brush: warm for rising, cool for falling, muted for
  /// steady. Direction-only semantics (no good/bad judgement), since "up" is desirable for a clock but
  /// not for a temperature — the color reads as motion, not severity.
  /// </summary>
  private static readonly SolidColorBrush Flat = Freeze(0x8A, 0x8A, 0x8A);

  /// <summary>
  /// Maps a <see cref="MetricTrend"/> to a frozen brush: warm for rising, cool for falling, muted for
  /// steady. Direction-only semantics (no good/bad judgement), since "up" is desirable for a clock but
  /// not for a temperature — the color reads as motion, not severity.
  /// </summary>
  /// <param name="value">The value to convert.</param>
  /// <param name="targetType">The type of the target.</param>
  /// <param name="parameter">The converter parameter.</param>
  /// <param name="culture">The culture to use.</param>
  /// <returns></returns>
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
    value is MetricTrend trend
      ? trend switch { MetricTrend.Rising => Rising, MetricTrend.Falling => Falling, _ => Flat }
      : Flat;

  /// <summary>
  /// Converts a value back to its original form. This method is not supported and will throw a NotSupportedException.
  /// </summary>
  /// <param name="value">The value to convert.</param>
  /// <param name="targetType">The type of the target.</param>
  /// <param name="parameter">The converter parameter.</param>
  /// <param name="culture">The culture to use.</param>
  /// <returns></returns>
  /// <exception cref="NotSupportedException"></exception>
  /// <returns></returns>
  /// <exception cref="NotSupportedException"></exception>
  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
    throw new NotSupportedException();

  /// <summary>
  /// Creates a frozen <see cref="SolidColorBrush"/> from the specified RGB values. 
  /// Freezing the brush improves performance and reduces memory usage when the brush is shared across multiple controls.
  /// </summary>
  /// <param name="r">The red component.</param>
  /// <param name="g">The green component.</param>
  /// <param name="b">The blue component.</param>
  /// <returns></returns>
  private static SolidColorBrush Freeze(byte r, byte g, byte b) {
    var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
    brush.Freeze();
    return brush;
  }
}
