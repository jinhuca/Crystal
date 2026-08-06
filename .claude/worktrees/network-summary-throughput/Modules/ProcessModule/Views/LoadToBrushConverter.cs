using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ProcessModule.Views;

/// <summary>
/// Maps a 0-100 load percentage to a heat color (green → amber → red), the HWiNFO cue for
/// spotting a busy process at a glance. Returns a frozen brush per band so the rows share three
/// instances rather than allocating on every poll.
/// </summary>
public sealed class LoadToBrushConverter : IValueConverter {
  private static readonly SolidColorBrush Low = Freeze(0x3B, 0xD1, 0x5A);   // green
  private static readonly SolidColorBrush Mid = Freeze(0xE8, 0xB3, 0x2A);   // amber
  private static readonly SolidColorBrush High = Freeze(0xE8, 0x4A, 0x3B);  // red

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    double load = value is double d ? d : 0;
    if (load >= 50) return High;
    if (load >= 20) return Mid;
    return Low;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();

  private static SolidColorBrush Freeze(byte r, byte g, byte b) {
    var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
    brush.Freeze();
    return brush;
  }
}
