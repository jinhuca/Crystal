using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;

namespace Crystal.Plot2D.Axes.Numeric;

/// <summary>
/// Represents a ticks provider for logarithmically transformed axis - returns ticks which are a power of specified logarithm base.
/// </summary>
public class LogarithmNumericTicksProvider : ITicksProvider<double> {
  /// <summary>
  /// Initializes a new instance of the <see cref="LogarithmNumericTicksProvider"/> class.
  /// </summary>
  public LogarithmNumericTicksProvider() {
    minorProvider = new MinorNumericTicksProvider(parent: this);
    minorProvider.Changed += ticksProvider_Changed;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="LogarithmNumericTicksProvider"/> class.
  /// </summary>
  /// <param name="logarithmBase">The logarithm base.</param>
  public LogarithmNumericTicksProvider(double logarithmBase)
    : this() {
    LogarithmBase = logarithmBase;
  }

  private void ticksProvider_Changed(object sender, EventArgs e) {
    Changed.Raise(sender: this);
  }

  private double logarithmBase = 10;
  public double LogarithmBase {
    get => logarithmBase;
    set {
      if(value <= 0) {
        throw new ArgumentOutOfRangeException(paramName: Strings.Exceptions.LogarithmBaseShouldBePositive);
      }

      logarithmBase = value;
    }
  }

  private double LogByBase(double d) {
    return Math.Log10(d: d) / Math.Log10(d: logarithmBase);
  }

  #region ITicksProvider<double> Members

  private double[] ticks;
  public ITicksInfo<double> GetTicks(Range<double> range, int ticksCount) {
    var min = LogByBase(d: range.Min);
    var max = LogByBase(d: range.Max);

    var minDown = Math.Floor(d: min);
    var maxUp = Math.Ceiling(a: max);

    var logLength = LogByBase(d: range.GetLength());

    ticks = CreateTicks(range: range);

    var log = RoundingHelper.GetDifferenceLog(min: range.Min, max: range.Max);
    TicksInfo<double> result = new() { Ticks = ticks, TickSizes = ArrayExtensions.CreateArray(length: ticks.Length, defaultValue: 1.0), Info = log };
    return result;
  }

  private double[] CreateTicks(Range<double> range) {
    var min = LogByBase(d: range.Min);
    var max = LogByBase(d: range.Max);

    var minDown = Math.Floor(d: min);
    var maxUp = Math.Ceiling(a: max);

    var intStart = (int)Math.Floor(d: minDown);
    var count = (int)(maxUp - minDown + 1);

    var ticks = new double[count];
    for(var i = 0; i < count; i++) {
      ticks[i] = intStart + i;
    }

    for(var i = 0; i < ticks.Length; i++) {
      ticks[i] = Math.Pow(x: logarithmBase, y: ticks[i]);
    }

    return ticks;
  }

  public int DecreaseTickCount(int ticksCount) {
    return ticksCount;
  }

  public int IncreaseTickCount(int ticksCount) {
    return ticksCount;
  }

  private readonly MinorNumericTicksProvider minorProvider;
  public ITicksProvider<double> MinorProvider {
    get {
      minorProvider.SetRanges(ranges: ArrayExtensions.GetPairs(array: ticks));
      return minorProvider;
    }
  }

  public ITicksProvider<double> MajorProvider => null;

  public event EventHandler Changed;

  #endregion
}
