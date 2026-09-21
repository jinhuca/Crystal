using System.Globalization;
using System.Windows.Data;

namespace Crystal.Controls.Metrics;

/// <summary>
/// Maps a <see cref="MetricTrend"/> to its glyph: ▲ rising, ▼ falling, – steady. The compact
/// direction cue a sparkline used to give, driven off a metric's per-poll trend.
/// </summary>
[ValueConversion(typeof(MetricTrend), typeof(string))]
public sealed class TrendToGlyphConverter : IValueConverter {
  /// <summary>
  /// Maps a <see cref="MetricTrend"/> to its glyph: ▲ rising, ▼ falling, – steady. The compact
  /// direction cue a sparkline used to give, driven off a metric's per-poll trend.
  /// </summary>
  /// <param name="value">The <see cref="MetricTrend"/> to convert.</param>
  /// <param name="targetType">The type of the target.</param>
  /// <param name="parameter">The converter parameter.</param>
  /// <param name="culture">The culture to use.</param>
  /// <returns>The glyph representing the trend.</returns>
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
    value is MetricTrend trend
      ? trend switch { MetricTrend.Rising => "▲", MetricTrend.Falling => "▼", _ => "–" }
      : "–";

  /// <summary>
  /// Converts a value back to its original form. This method is not supported and will throw a NotSupportedException.
  /// </summary>
  /// <param name="value">The value to convert back.</param>
  /// <param name="targetType">The type of the target.</param>
  /// <param name="parameter">The converter parameter.</param>
  /// <param name="culture">The culture to use.</param>
  /// <returns></returns>
  /// <exception cref="NotSupportedException"></exception>
  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
    throw new NotSupportedException();
}
