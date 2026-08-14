using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Crystal.StorageModule.Converters;

/// <summary>Converts a 0-1 fraction to a star <see cref="GridLength"/> so the capacity bar's
/// used/free columns size in proportion to one another. The two fractions sum to 1, so the star
/// weights map directly to width shares.</summary>
public sealed class FractionToStarConverter : IValueConverter {
  public object Convert(object value, Type targetType, object? parameter, CultureInfo culture) =>
      new GridLength(value is double d && d > 0 ? d : 0, GridUnitType.Star);

  public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}
