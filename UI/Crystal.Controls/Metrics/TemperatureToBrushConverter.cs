using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Crystal.Controls.Metrics;

/// <summary>
/// Maps a temperature (°C) to a severity brush: normal below 75, warm 75–90, hot at/above 90. The
/// "normal" band uses the theme text color so a cool part reads exactly like the other tiles; only a
/// genuinely warm/hot reading is tinted, keeping the color meaningful rather than ever-present.
/// </summary>
public sealed class TemperatureToBrushConverter : IValueConverter {
  /// <summary>
  /// Maps a temperature (°C) to a severity brush: normal below 75, warm 75–90, hot at/above 90. The
  /// "normal" band uses the theme text color so a cool part reads exactly like the other tiles; only a
  /// genuinely warm/hot reading is tinted, keeping the color meaningful rather than ever-present.
  /// </summary>
  private static readonly SolidColorBrush Warm = Freeze(0xE8, 0xB3, 0x2A);

  /// <summary>
  /// Maps a temperature (°C) to a severity brush: normal below 75, warm 75–90, hot at/above 90. The
  /// "normal" band uses the theme text color so a cool part reads exactly like the other tiles; only a
  /// genuinely warm/hot reading is tinted, keeping the color meaningful rather than ever-present.
  /// </summary>
  private static readonly SolidColorBrush Hot = Freeze(0xE8, 0x4A, 0x3B);

  /// <summary>
  /// Maps a temperature (°C) to a severity brush: normal below 75, warm 75–90, hot at/above 90. The
  /// "normal" band uses the theme text color so a cool part reads exactly like the other tiles; only a
  /// genuinely warm/hot reading is tinted, keeping the color meaningful rather than ever-present.
  /// </summary>
  /// <param name="value">The temperature (°C) to convert.</param>
  /// <param name="targetType">The type of the target.</param>
  /// <param name="parameter">The converter parameter.</param>
  /// <param name="culture">The culture to use.</param>
  /// <returns>The brush representing the severity.</returns>
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    double temp = value switch { double d => d, _ => 0 };
    if (temp >= 90) return Hot;
    if (temp >= 75) return Warm;
    return Application.Current?.TryFindResource("TextBrush") as Brush ?? Freeze(0xE6, 0xE6, 0xE6);
  }

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

  /// <summary>
  /// Creates a frozen SolidColorBrush from the specified RGB values. Freezing the brush improves performance and 
  /// reduces memory usage when the brush is shared across multiple controls.
  /// </summary>
  /// <param name="r">The red component.</param>
  /// <param name="g">The green component.</param>
  /// <param name="b">The blue component.</param>
  /// <returns>The frozen SolidColorBrush.</returns>
  private static SolidColorBrush Freeze(byte r, byte g, byte b) {
    var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
    brush.Freeze();
    return brush;
  }
}
