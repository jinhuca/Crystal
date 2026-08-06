using System.Windows.Media;

namespace Crystal.Controls.MeterGauges.Themes;

/// <summary>Built-in <see cref="GaugeTheme"/> presets for <see cref="MeterGauge"/>.</summary>
public static class GaugeThemes {
  private static readonly Brush DefaultBackground = Freeze(Brushes.Black);
  private static readonly Brush DefaultInactive = Freeze(new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x40)));

  /// <summary>Emerald/green accent — matches the reference "Voltage" gauge.</summary>
  public static GaugeTheme Emerald() => FromAccent(Color.FromRgb(0x3B, 0xD1, 0x5A));

  /// <summary>Rose/magenta accent.</summary>
  public static GaugeTheme Rose() => FromAccent(Color.FromRgb(0xE8, 0x2A, 0x7A));

  /// <summary>Amber accent — a common "warning" color.</summary>
  public static GaugeTheme Amber() => FromAccent(Color.FromRgb(0xE8, 0x9B, 0x2A));

  /// <summary>Sky-blue accent.</summary>
  public static GaugeTheme Sky() => FromAccent(Color.FromRgb(0x3E, 0x9B, 0xE8));

  /// <summary>Builds a theme from a single accent color used for the lit ticks, over a shared dark backdrop.</summary>
  public static GaugeTheme FromAccent(Color accent) {
    return new GaugeTheme {
      GaugeBackground = DefaultBackground,
      InactiveBrush = DefaultInactive,
      ActiveBrush = Freeze(new SolidColorBrush(accent))
    };
  }

  private static Brush Freeze(Brush brush) {
    if (brush.CanFreeze) brush.Freeze();
    return brush;
  }
}
