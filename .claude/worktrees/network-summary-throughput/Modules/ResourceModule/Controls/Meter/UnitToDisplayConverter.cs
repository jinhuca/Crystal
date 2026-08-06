using System.Globalization;
using System.Windows.Data;

namespace ResourceModule.Controls.Meter;

[ValueConversion(typeof(Unit), typeof(string))]
public class UnitToDisplayConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    var abs = Definitions.AbsoluteString ?? string.Empty;
    var pct = Definitions.PercentageString ?? string.Empty;
    var none = Definitions.NoneString ?? string.Empty;
    var ghz = Definitions.GHzString ?? string.Empty;
    var celsius = Definitions.CelsiusString ?? string.Empty;

    var input = value?.ToString() ?? string.Empty;

    if (Enum.TryParse<Unit>(input, out Unit unit_)) {
      return unit_ switch {
        Unit.Percent => pct,
        Unit.Absolute => parameter?.ToString() ?? abs,
        Unit.GHz => ghz,
        Unit.Celsius => celsius,
        Unit.Volts => Definitions.VoltsString ?? string.Empty,
        Unit.Watts => Definitions.WattsString ?? string.Empty,
        Unit.None => none,
        _ => string.Empty,
      };
    }
    return abs;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    throw new NotSupportedException();
  }
}