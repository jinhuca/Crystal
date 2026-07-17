using System;
using System.Windows.Data;

namespace Crystal.Plot2D.Converters;

public abstract class ThreeValuesMultiConverter<T1, T2, T3> : IMultiValueConverter {
  #region IMultiValueConverter Members

  public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
    if(values != null && values.Length == 3) {
      if(values[0] is T1 && values[1] is T2 && values[2] is T3) {
        var param1 = (T1)values[0];
        var param2 = (T2)values[1];
        var param3 = (T3)values[2];
        return ConvertCore(value1: param1, value2: param2, value3: param3);
      }
    }

    return null;
  }

  protected abstract object ConvertCore(T1 value1, T2 value2, T3 value3);

  public virtual object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) {
    throw new NotSupportedException();
  }

  #endregion
}
