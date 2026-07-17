using System;
using System.Windows;
using System.Windows.Data;

namespace Crystal.Plot2D.Converters;

public abstract class FourValuesMultiConverter<T1, T2, T3, T4> : IMultiValueConverter {
  #region IMultiValueConverter Members

  public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
    if(values != null && values.Length == 4) {
      if(values[0] is T1 && values[1] is T2 && values[2] is T3 && values[3] is T4) {
        var param1_ = (T1)values[0];
        var param2_ = (T2)values[1];
        var param3_ = (T3)values[2];
        var param4_ = (T4)values[3];

        var result_ = ConvertCore(value1: param1_, value2: param2_, value3: param3_, value4: param4_, targetType: targetType, parameter: parameter, culture: culture);
        return result_;
      }
    }

    return DependencyProperty.UnsetValue;
  }

  protected abstract object ConvertCore(T1 value1, T2 value2, T3 value3, T4 value4, Type targetType, object parameter, System.Globalization.CultureInfo culture);

  public virtual object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) => throw new NotSupportedException();

  #endregion
}
