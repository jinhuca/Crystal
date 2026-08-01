
using System.Globalization;
using System.Windows.Data;

namespace Crystal.WpfConverters;

[ValueConversion(typeof(double), typeof(double))]
public class MHzToGHzConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    return Math.Round(System.Convert.ToDouble(value) / 1000, 2);
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    throw new NotImplementedException();
  }
}
