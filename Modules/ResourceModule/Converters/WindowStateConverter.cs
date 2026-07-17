using System.Globalization;
using System.Windows.Data;

namespace ResourceModule.Converters;

public class WindowStateConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    string result_ = string.Empty;
    if (value is System.Windows.WindowState state) {
      switch(state) {
        case System.Windows.WindowState.Normal:
          result_ = "\uE922";
          break;
        case System.Windows.WindowState.Minimized:
          result_ = "\uE921";
          break;
        case System.Windows.WindowState.Maximized:
          result_ = "\uE923";
          break;
      }
    }
    return result_;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    throw new NotImplementedException();
  }
}
