using System;
using System.ComponentModel;
using System.Globalization;

namespace Crystal.Plot2D.Common;

public sealed class RangeConverter : TypeConverter {
  public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
    return sourceType == typeof(string) || base.CanConvertFrom(context: context, sourceType: sourceType);
  }

  public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
    return destinationType == typeof(string) || base.CanConvertTo(context: context, destinationType: destinationType);
  }

  public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
    if(value == null) {
      throw GetConvertFromException(nameof(value));
    }

    if(value is string source_) {
      var parts_ = source_.Split(separator: '-');
      var minStr_ = parts_[0];
      var maxStr_ = parts_[1];

      if(int.TryParse(s: minStr_, style: NumberStyles.Integer, provider: culture, result: out var minInt32_)) {
        var maxInt32_ = int.Parse(s: maxStr_, style: NumberStyles.Integer, provider: culture);

        return new Range<int>(min: minInt32_, max: maxInt32_);
      }

      if(double.TryParse(s: minStr_, style: NumberStyles.Float, provider: culture, result: out var minDouble_)) {
        var maxDouble_ = double.Parse(s: maxStr_, style: NumberStyles.Float, provider: culture);
        return new Range<double>(min: minDouble_, max: maxDouble_);
      }

      if(DateTime.TryParse(s: minStr_, provider: culture, styles: DateTimeStyles.None, result: out var minDateTime_)) {
        var maxDateTime_ = DateTime.Parse(s: maxStr_, provider: culture);
        return new Range<DateTime>(min: minDateTime_, max: maxDateTime_);
      }
    }

    return base.ConvertFrom(context: context, culture: culture, value: value);
  }

  public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
    return value switch {
      DataRect dataRect_ when destinationType == typeof(string) => dataRect_.ConvertToString(format: null, provider: culture),
      _ => base.ConvertTo(context: context, culture: culture, value: value, destinationType: destinationType)
    };
  }
}
