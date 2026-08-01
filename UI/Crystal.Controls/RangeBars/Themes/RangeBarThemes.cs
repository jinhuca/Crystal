using System.Windows.Media;

namespace Crystal.Controls.RangeBars.Themes;

/// <summary>Built-in <see cref="RangeBarTheme"/> presets for <see cref="RangeBar"/>.</summary>
public static class RangeBarThemes {
  private static readonly Brush DefaultBackground = Freeze(Brushes.Black);
  private static readonly Brush DefaultTrack = Freeze(new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x40)));
  private static readonly Brush DefaultBorder = Freeze(Brushes.Black);

  /// <summary>Emerald/green accent.</summary>
  public static RangeBarTheme Emerald() => FromAccent(Color.FromRgb(0x3B, 0xD1, 0x5A));

  /// <summary>Rose/magenta accent.</summary>
  public static RangeBarTheme Rose() => FromAccent(Color.FromRgb(0xE8, 0x2A, 0x7A));

  /// <summary>Amber accent — a common "warning" color.</summary>
  public static RangeBarTheme Amber() => FromAccent(Color.FromRgb(0xE8, 0x9B, 0x2A));

  /// <summary>Sky-blue accent.</summary>
  public static RangeBarTheme Sky() => FromAccent(Color.FromRgb(0x3E, 0x9B, 0xE8));

  /// <summary>Builds a theme from a single accent color used for the fill, over a shared dark track.</summary>
  public static RangeBarTheme FromAccent(Color accent) {
    return new RangeBarTheme {
      BarBackground = DefaultBackground,
      TrackBrush = DefaultTrack,
      BorderBrush = DefaultBorder,
      FillBrush = Freeze(new SolidColorBrush(accent))
    };
  }

  private static Brush Freeze(Brush brush) {
    if (brush.CanFreeze) brush.Freeze();
    return brush;
  }
}
