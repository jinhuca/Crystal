using System;
using System.Windows;
using System.Windows.Data;

namespace Crystal.Plot2D.Converters;

public abstract class TwoValuesMultiConverter<T1, T2> : IMultiValueConverter {
  #region IMultiValueConverter Members

  public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
    if(values != null && values.Length == 2) {
      if(values[0] is T1 && values[1] is T2) {
        var param1 = (T1)values[0];
        var param2 = (T2)values[1];
        return ConvertCore(value1: param1, value2: param2, targetType: targetType, parameter: parameter, culture: culture);
      }
    }

    return DependencyProperty.UnsetValue;
  }

  protected abstract object ConvertCore(T1 value1, T2 value2, Type targetType, object parameter, System.Globalization.CultureInfo culture);

  public virtual object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) => throw new NotSupportedException();

  #endregion
}
