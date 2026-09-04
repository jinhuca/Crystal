using System;
using System.Globalization;
using System.Windows.Data;
using Crystal.Controls.PerformanceGraphs;

namespace Crystal.Controls.Converters;

/// <summary>
/// Computes the Width a <see cref="PerformanceGraphLite"/> needs so its dots render perfectly
/// square, given a fixed Height plus its own Capacity/Rows - the exact inverse of
/// <see cref="CapacityToRatioConverter"/> (which derives Height from Width instead).
/// </summary>
/// <remarks>
/// Fixing Height and deriving Width is what lets several graphs of different Capacity share one
/// identical dot-cell size: equal Height and Rows give an equal row pitch, and because the dots
/// are square that pins the column pitch too, so each graph's width simply scales with its own
/// Capacity (a 120-sample graph comes out exactly twice as wide as a 60-sample one, same cells).
/// Bind all three inputs from the SAME element via a MultiBinding (Height, Capacity, Rows).
/// </remarks>
public class SquareDotWidthConverter : IMultiValueConverter {
  public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
    // Safe placeholder while WPF is still resolving initial layout sizes, or if this converter
    // gets bound to something other than the 3-value (Height, Capacity, Rows) shape it expects.
    if (values.Length < 3
        || values[0] is not double height || double.IsNaN(height) || height <= 0
        || values[1] is not int capacity
        || values[2] is not int rows) {
      return 20.0;
    }

    // SquareDotAspectRatio is Height/Width for square dots, so Width = Height / ratio.
    double ratio = PerformanceGraphLite.SquareDotAspectRatio(rows, capacity);
    return Math.Max(20.0, height / ratio);
  }

  public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}
