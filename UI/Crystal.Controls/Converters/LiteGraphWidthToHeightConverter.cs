using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Crystal.Controls.Converters;

[ValueConversion(typeof(double), typeof(double))]
public class LiteGraphWidthToHeightConverter : IValueConverter {
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

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    throw new NotImplementedException();
  }
}
