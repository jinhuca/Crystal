using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Crystal.CpuModule.Views;

/// <summary>
/// Maps a 0-100 load percentage to a heat color (green → amber → red), the HWiNFO cue for
/// spotting a hot core at a glance. Thresholds are coarse on purpose: idle/light stays green,
/// sustained work goes amber, near-saturated goes red. Returns a frozen brush per band so the
/// per-core rows share three instances rather than allocating on every sensor tick.
/// </summary>
public sealed class LoadToBrushConverter : IValueConverter {
  /// <summary>
  /// The low load color (green) for loads below 60%.
  /// </summary>
  private static readonly SolidColorBrush Low = Freeze(0x3B, 0xD1, 0x5A);   // green

  /// <summary>
  /// The mid load color (amber) for loads between 60% and 85%.
  /// </summary>
  private static readonly SolidColorBrush Mid = Freeze(0xE8, 0xB3, 0x2A);   // amber

  /// <summary>
  /// The high load color (red) for loads above 85%.
  /// </summary>
  private static readonly SolidColorBrush High = Freeze(0xE8, 0x4A, 0x3B);  // red

  /// <summary>
  /// Converts a load percentage to a heat color brush based on predefined thresholds.
  /// </summary>
  /// <param name="value">The load percentage.</param>
  /// <param name="targetType">The target type.</param>
  /// <param name="parameter">The parameter.</param>
  /// <param name="culture">The culture.</param>
  /// <returns>The heat color brush.</returns>
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    double load = value is double d ? d : 0;
    if (load >= 85) return High;
    if (load >= 60) return Mid;
    return Low;
  }

  /// <summary>
  /// Not supported. This converter does not support converting back from a brush to a load percentage.
  /// </summary>
  /// <param name="value">The brush.</param>
  /// <param name="targetType">The target type.</param>
  /// <param name="parameter">The parameter.</param>
  /// <param name="culture">The culture.</param>
  /// <returns></returns>
  /// <exception cref="NotSupportedException"></exception>
  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
    throw new NotSupportedException();

  /// <summary>
  /// Creates a frozen SolidColorBrush from the specified RGB values. Freezing the brush improves performance 
  /// by making it immutable and shareable across multiple UI elements.
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
