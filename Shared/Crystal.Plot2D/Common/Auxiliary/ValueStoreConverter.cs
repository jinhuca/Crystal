using System;
using System.Globalization;
using System.Windows.Data;

namespace Crystal.Plot2D.Common.Auxiliary;

public sealed class ValueStoreConverter : IValueConverter {
  #region IValueConverter Members

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    var store_ = (ValueStore)value;
    var key_ = (string)parameter;

    return store_[propertyName: key_];
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    throw new NotSupportedException();
  }

  #endregion
}
