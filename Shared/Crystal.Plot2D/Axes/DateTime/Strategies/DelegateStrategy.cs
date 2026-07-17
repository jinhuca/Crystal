using System;

namespace Crystal.Plot2D.Axes;

public class DelegateDateTimeStrategy : DefaultDateTimeTicksStrategy {
  private readonly Func<System.TimeSpan, DifferenceIn?> function;

  public DelegateDateTimeStrategy(Func<System.TimeSpan, DifferenceIn?> function) {
    ArgumentNullException.ThrowIfNull(function);
    this.function = function;
  }

  public override DifferenceIn GetDifference(System.TimeSpan span) {
    var customResult_ = function(arg: span);
    var result_ = customResult_ ?? base.GetDifference(span: span);
    return result_;
  }
}
