using System.Globalization;
using System.Windows.Data;

namespace Crystal.Controls.Converters;

/// <summary>
/// Converts a parent width to a corresponding height for a LiteGraph control, maintaining an aspect 
/// ratio of approximately 0.618 (the golden ratio). The converter subtracts a total horizontal margin
/// of 20 units (10 units on each side) from the parent width before calculating the height. If the 
/// parent width is less than or equal to 20, the converter returns 0.0 to avoid negative or zero height values.
/// </summary>
[ValueConversion(typeof(double), typeof(double))]
public class LiteGraphWidthToHeightConverter : IValueConverter {
  /// <summary>
  /// Converts a parent width to a corresponding height for a LiteGraph control, maintaining an aspect ratio of 
  /// approximately 0.618 (the golden ratio). The converter subtracts a total horizontal margin of 20 units 
  /// (10 units on each side) from the parent width before calculating the height.
  /// </summary>
  /// <param name="value">The parent width.</param>
  /// <param name="targetType">The type of the target.</param>
  /// <param name="parameter">The converter parameter.</param>
  /// <param name="culture">The culture to use.</param>
  /// <returns>The corresponding height.</returns>
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (value is double parentWidth && parentWidth > 0) {
      // Subtract total horizontal margin (10 left + 10 right = 20)
      double usableWidth = parentWidth - 20;

      if (usableWidth > 0) {
        return usableWidth * 0.618;
      }
    }

    return 0.0;
  }

  /// <summary>
  /// Not implemented. This converter does not support converting back from height to width.
  /// </summary>
  /// <param name="value"></param>
  /// <param name="targetType"></param>
  /// <param name="parameter"></param>
  /// <param name="culture"></param>
  /// <returns></returns>
  /// <exception cref="NotImplementedException"></exception>
  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    throw new NotImplementedException();
  }
}
