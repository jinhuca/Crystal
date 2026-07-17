using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crystal.Plot2D.Axes.Numeric;

/// <summary>
/// Represents a ticks provider for <see cref="System.Double"/> values.
/// </summary>
public sealed class NumericTicksProvider : ITicksProvider<double> {
  /// <summary>
  /// Initializes a new instance of the <see cref="NumericTicksProvider"/> class.
  /// </summary>
  public NumericTicksProvider() {
    minorProvider = new MinorNumericTicksProvider(parent: this);
    minorProvider.Changed += minorProvider_Changed;
    minorProvider.Coeffs = new double[] { 0.3, 0.3, 0.3, 0.3, 0.6, 0.3, 0.3, 0.3, 0.3 };
  }

  private void minorProvider_Changed(object sender, EventArgs e) {
    Changed.Raise(sender: this);
  }

  public event EventHandler Changed;
  private void RaiseChangedEvent() {
    Changed.Raise(sender: this);
  }

  private double minStep;
  /// <summary>
  /// Gets or sets the minimal step between ticks.
  /// </summary>
  /// <value>The min step.</value>
  public double MinStep {
    get => minStep;
    set {
      Verify.IsTrue(condition: value >= 0.0, paramName: "value");
      if(minStep != value) {
        minStep = value;
        RaiseChangedEvent();
      }
    }
  }

  private double[] ticks;
  public ITicksInfo<double> GetTicks(Range<double> range, int ticksCount) {
    var start = range.Min;
    var finish = range.Max;

    var delta = finish - start;

    var log = (int)Math.Round(a: Math.Log10(d: delta));

    var newStart = RoundingHelper.Round(number: start, rem: log);
    var newFinish = RoundingHelper.Round(number: finish, rem: log);
    if(newStart == newFinish) {
      log--;
      newStart = RoundingHelper.Round(number: start, rem: log);
      newFinish = RoundingHelper.Round(number: finish, rem: log);
    }

    // calculating step between ticks
    var unroundedStep = (newFinish - newStart) / ticksCount;
    var stepLog = log;
    // trying to round step
    var step = RoundingHelper.Round(number: unroundedStep, rem: stepLog);
    if(step == 0) {
      stepLog--;
      step = RoundingHelper.Round(number: unroundedStep, rem: stepLog);
      if(step == 0) {
        // step will not be rounded if attempts to be rounded to zero.
        step = unroundedStep;
      }
    }

    if(step < minStep) {
      step = minStep;
    }

    ticks = step != 0.0 ? CreateTicks(start: start, finish: finish, step: step) : new double[] { };

    TicksInfo<double> res = new() { Info = log, Ticks = ticks };

    return res;
  }

  private static double[] CreateTicks(double start, double finish, double step) {
    DebugVerify.Is(condition: step != 0.0);

    var x = step * Math.Floor(d: start / step);

    if(x == x + step) {
      return Array.Empty<double>();
    }

    List<double> res = new();

    var increasedFinish = finish + step * 1.05;
    while(x <= increasedFinish) {
      res.Add(item: x);
      DebugVerify.Is(condition: res.Count < 2000);
      x += step;
    }
    return res.ToArray();
  }

  private static readonly int[] tickCounts = { 20, 10, 5, 4, 2, 1 };

  public const int DefaultPreferredTicksCount = 10;

  public int DecreaseTickCount(int ticksCount) {
    return tickCounts.FirstOrDefault(predicate: tick => tick < ticksCount);
  }

  public int IncreaseTickCount(int ticksCount) {
    var newTickCount = tickCounts.Reverse().FirstOrDefault(predicate: tick => tick > ticksCount);
    if(newTickCount == 0) {
      newTickCount = tickCounts[0];
    }

    return newTickCount;
  }

  private readonly MinorNumericTicksProvider minorProvider;
  public ITicksProvider<double> MinorProvider {
    get {
      if(ticks != null) {
        minorProvider.SetRanges(ranges: ticks.GetPairs());
      }

      return minorProvider;
    }
  }

  public ITicksProvider<double> MajorProvider => null;
}
