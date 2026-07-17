using System;
using System.Globalization;
using System.Windows.Data;

namespace Crystal.Plot2D.Converters;

public class GenericValueConverter<T> : IValueConverter {
  protected GenericValueConverter() { }

  private Func<T, object> Conversion { get; }

  public GenericValueConverter(Func<T, object> conversion) {
    Conversion = conversion ?? throw new ArgumentNullException(paramName: nameof(conversion));
  }

  #region IValueConverter Members

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if(value is not T genericValue_) return null;
    return ConvertCore(value: genericValue_);
  }

  protected virtual object ConvertCore(T value) {
    if(Conversion != null) {
      return Conversion(arg: value);
    }

    throw new NotImplementedException();
  }

  public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    throw new NotSupportedException();
  }

  #endregion
}
