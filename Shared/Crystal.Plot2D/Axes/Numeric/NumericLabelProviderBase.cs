using System;
using System.Globalization;

namespace Crystal.Plot2D.Axes.Numeric;

public abstract class NumericLabelProviderBase : LabelProviderBase<double> {
  private bool shouldRound = true;
  private int rounding;

  protected void Init(double[] ticks) {
    if(ticks.Length == 0) {
      return;
    }

    var start_ = ticks[0];
    var finish_ = ticks[ticks.Length - 1];

    if(Math.Abs(start_ - finish_) < Constants.Constants.FloatComparisonTolerance) {
      shouldRound = false;
      return;
    }

    var delta_ = finish_ - start_;

    rounding = (int)Math.Round(a: Math.Log10(d: delta_));

    var newStart_ = RoundingHelper.Round(number: start_, rem: rounding);
    var newFinish_ = RoundingHelper.Round(number: finish_, rem: rounding);
    if(Math.Abs(newStart_ - newFinish_) < Constants.Constants.FloatComparisonTolerance) {
      rounding--;
    }
  }

  protected override string GetStringCore(LabelTickInfo<double> tickInfo) {
    string res_;
    if(!shouldRound) {
      res_ = tickInfo.Tick.ToString(provider: CultureInfo.InvariantCulture);
    }
    else {
      var round_ = Math.Min(val1: 15, val2: Math.Max(val1: -15, val2: rounding - 3)); // was rounding - 2
      res_ = RoundingHelper.Round(number: tickInfo.Tick, rem: round_).ToString(provider: CultureInfo.InvariantCulture);
    }

    return res_;
  }
}
