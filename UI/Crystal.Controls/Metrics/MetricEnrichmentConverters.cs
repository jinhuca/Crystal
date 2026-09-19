using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Crystal.Controls.Metrics;

/// <summary>
/// Maps a <see cref="MetricTrend"/> to its glyph: ▲ rising, ▼ falling, – steady. The compact
/// direction cue a sparkline used to give, driven off a metric's per-poll trend.
/// </summary>
public sealed class TrendToGlyphConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
    value is MetricTrend trend
      ? trend switch { MetricTrend.Rising => "▲", MetricTrend.Falling => "▼", _ => "–" }
      : "–";

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
    throw new NotSupportedException();
}

/// <summary>
/// Maps a <see cref="MetricTrend"/> to a frozen brush: warm for rising, cool for falling, muted for
/// steady. Direction-only semantics (no good/bad judgement), since "up" is desirable for a clock but
/// not for a temperature — the color reads as motion, not severity.
/// </summary>
public sealed class TrendToBrushConverter : IValueConverter {
  private static readonly SolidColorBrush Rising = Freeze(0xE8, 0xB3, 0x2A);  // warm amber
  private static readonly SolidColorBrush Falling = Freeze(0x4A, 0xA3, 0xE8); // cool blue
  private static readonly SolidColorBrush Flat = Freeze(0x8A, 0x8A, 0x8A);    // muted grey

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
    value is MetricTrend trend
      ? trend switch { MetricTrend.Rising => Rising, MetricTrend.Falling => Falling, _ => Flat }
      : Flat;

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
    throw new NotSupportedException();

  private static SolidColorBrush Freeze(byte r, byte g, byte b) {
    var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
    brush.Freeze();
    return brush;
  }
}

/// <summary>
/// Maps a temperature (°C) to a severity brush: normal below 75, warm 75–90, hot at/above 90. The
/// "normal" band uses the theme text color so a cool part reads exactly like the other tiles; only a
/// genuinely warm/hot reading is tinted, keeping the color meaningful rather than ever-present.
/// </summary>
public sealed class TemperatureToBrushConverter : IValueConverter {
  private static readonly SolidColorBrush Warm = Freeze(0xE8, 0xB3, 0x2A); // amber
  private static readonly SolidColorBrush Hot = Freeze(0xE8, 0x4A, 0x3B);  // red

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    double temp = value switch { double d => d, _ => 0 };
    if (temp >= 90) return Hot;
    if (temp >= 75) return Warm;
    return Application.Current?.TryFindResource("TextBrush") as Brush ?? Freeze(0xE6, 0xE6, 0xE6);
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
    throw new NotSupportedException();

  private static SolidColorBrush Freeze(byte r, byte g, byte b) {
    var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
    brush.Freeze();
    return brush;
  }
}
