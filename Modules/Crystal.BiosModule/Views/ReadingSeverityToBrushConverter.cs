using Crystal.BiosModule.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Crystal.BiosModule.Views;

/// <summary>Maps a live board reading's <see cref="ReadingSeverity"/> to a value foreground:
/// neutral when in-spec, amber when out of tolerance, red when far out. Lets the "LIVE BOARD"
/// block flag an anomalous rail or a weak CMOS cell at a glance.</summary>
public sealed class ReadingSeverityToBrushConverter : IValueConverter {
  private static readonly SolidColorBrush Normal = Freeze(0xE6, 0xE6, 0xE6);
  private static readonly SolidColorBrush Warning = Freeze(0xE8, 0xB3, 0x3E);
  private static readonly SolidColorBrush Critical = Freeze(0xE8, 0x5C, 0x5C);

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      value switch {
        ReadingSeverity.Warning => Warning,
        ReadingSeverity.Critical => Critical,
        _ => Normal,
      };

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      Binding.DoNothing;

  private static SolidColorBrush Freeze(byte r, byte g, byte b) {
    var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
    brush.Freeze();
    return brush;
  }
}
