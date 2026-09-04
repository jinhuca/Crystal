using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Crystal.GpuModule.Views;

/// <summary>
/// Maps a tile's zero-based position to its left chrome: the first tile gets none, so a lone adapter
/// shows no divider and no leading gap; every later tile gets a left divider and gap. The requested
/// aspect ("border" for the divider line, otherwise the gap) is passed as the converter parameter.
/// </summary>
public sealed class AdapterTileChromeConverter : IValueConverter {
  /// <inheritdoc/>
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    var isFirst = value is not int index || index <= 0;
    if (isFirst) {
      return new Thickness(0);
    }

    return (parameter as string) == "border" ? new Thickness(1, 0, 0, 0) : new Thickness(16, 0, 0, 0);
  }

  /// <inheritdoc/>
  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}
