using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Diagnostics;

namespace Crystal.Plot2D.Axes;

internal static class RoundingHelper {
  internal static int GetDifferenceLog(double min, double max) {
    return (int)Math.Round(a: Math.Log10(d: Math.Abs(value: max - min)));
  }

  internal static double Round(double number, int rem) {
    if(rem <= 0) {
      rem = MathHelper.Clamp(value: -rem, min: 0, max: 15);
      return Math.Round(value: number, digits: rem);
    }

    var pow_ = Math.Pow(x: 10, y: rem - 1);
    var val_ = pow_ * Math.Round(a: number / Math.Pow(x: 10, y: rem - 1));
    return val_;
  }

  internal static double Round(double value, Range<double> range) {
    var log_ = GetDifferenceLog(min: range.Min, max: range.Max);

    return Round(number: value, rem: log_);
  }

  internal static RoundingInfo CreateRoundedRange(double min, double max) {
    var delta_ = max - min;

    if(delta_ == 0) {
      return new RoundingInfo { Min = min, Max = max, Log = 0 };
    }

    var log_ = (int)Math.Round(a: Math.Log10(d: Math.Abs(value: delta_))) + 1;

    var newMin_ = Round(number: min, rem: log_);
    var newMax_ = Round(number: max, rem: log_);
    if(newMin_ == newMax_) {
      log_--;
      newMin_ = Round(number: min, rem: log_);
      newMax_ = Round(number: max, rem: log_);
    }

    return new RoundingInfo { Min = newMin_, Max = newMax_, Log = log_ };
  }
}

[DebuggerDisplay(value: "{Min} - {Max}, Log = {Log}")]
internal sealed class RoundingInfo {
  public double Min { get; set; }
  public double Max { get; set; }
  public int Log { get; set; }
}
