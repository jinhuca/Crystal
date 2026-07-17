using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Windows.Markup;

namespace Crystal.Plot2D.Axes.Numeric;

[ContentProperty(name: "TicksProvider")]
public class CustomBaseNumericTicksProvider : ITicksProvider<double> {
  private double customBase = 2;

  /// <summary>
  /// Gets or sets the custom base.
  /// </summary>
  /// <value>The custom base.</value>
  public double CustomBase {
    get => customBase;
    set {
      if(double.IsNaN(d: value)) {
        throw new ArgumentException(message: Strings.Exceptions.CustomBaseTicksProviderBaseIsNaN);
      }

      if(value <= 0) {
        throw new ArgumentOutOfRangeException(paramName: Strings.Exceptions.CustomBaseTicksProviderBaseIsTooSmall);
      }

      customBase = value;
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="CustomBaseNumericTicksProvider"/> class.
  /// </summary>
  public CustomBaseNumericTicksProvider() : this(customBase: 2.0) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="CustomBaseNumericTicksProvider"/> class.
  /// </summary>
  /// <param name="customBase">The custom base, e.g. Math.PI</param>
  public CustomBaseNumericTicksProvider(double customBase) : this(customBase: customBase, ticksProvider: new NumericTicksProvider()) { }

  private CustomBaseNumericTicksProvider(double customBase, ITicksProvider<double> ticksProvider) {
    ArgumentNullException.ThrowIfNull(ticksProvider);

    CustomBase = customBase;

    TicksProvider = ticksProvider;
  }

  private void ticksProvider_Changed(object sender, EventArgs e) {
    Changed.Raise(sender: this);
  }

  private ITicksProvider<double> ticksProvider;
  public ITicksProvider<double> TicksProvider {
    get => ticksProvider;
    set {
      ArgumentNullException.ThrowIfNull(value);

      if(ticksProvider != null) {
        ticksProvider.Changed -= ticksProvider_Changed;
      }

      ticksProvider = value;
      ticksProvider.Changed += ticksProvider_Changed;

      if(minorTicksProvider != null) {
        minorTicksProvider.Changed -= minorTicksProvider_Changed;
      }

      minorTicksProvider = new MinorProviderWrapper(owner: this);
      minorTicksProvider.Changed += minorTicksProvider_Changed;

      Changed.Raise(sender: this);
    }
  }

  private void minorTicksProvider_Changed(object sender, EventArgs e) {
    Changed.Raise(sender: this);
  }

  private Range<double> TransformRange(Range<double> range) {
    var min = range.Min / customBase;
    var max = range.Max / customBase;

    return new Range<double>(min: min, max: max);
  }

  #region ITicksProvider<double> Members

  private double[] tickMarks;
  public ITicksInfo<double> GetTicks(Range<double> range, int ticksCount) {
    var ticks = ticksProvider.GetTicks(range: TransformRange(range: range), ticksCount: ticksCount);

    TransformTicks(ticks: ticks);

    tickMarks = ticks.Ticks;

    return ticks;
  }

  private void TransformTicks(ITicksInfo<double> ticks) {
    for(var i = 0; i < ticks.Ticks.Length; i++) {
      ticks.Ticks[i] *= customBase;
    }
  }

  public int DecreaseTickCount(int ticksCount) {
    return ticksProvider.DecreaseTickCount(ticksCount: ticksCount);
  }

  public int IncreaseTickCount(int ticksCount) {
    return ticksProvider.IncreaseTickCount(ticksCount: ticksCount);
  }

  private ITicksProvider<double> minorTicksProvider;
  public ITicksProvider<double> MinorProvider => minorTicksProvider;

  /// <summary>
  /// Gets the major provider, used to generate major ticks - for example, years for common ticks as months.
  /// </summary>
  /// <value>The major provider.</value>
  public ITicksProvider<double> MajorProvider => null;

  public event EventHandler Changed;

  #endregion

  private sealed class MinorProviderWrapper : ITicksProvider<double> {
    private readonly CustomBaseNumericTicksProvider owner;

    public MinorProviderWrapper(CustomBaseNumericTicksProvider owner) {
      this.owner = owner;

      MinorTicksProvider.Changed += MinorTicksProvider_Changed;
    }

    private void MinorTicksProvider_Changed(object sender, EventArgs e) {
      Changed.Raise(sender: this);
    }

    private ITicksProvider<double> MinorTicksProvider => owner.ticksProvider.MinorProvider;

    #region ITicksProvider<double> Members

    public ITicksInfo<double> GetTicks(Range<double> range, int ticksCount) {
      var minorProvider = MinorTicksProvider;
      var ticks = minorProvider.GetTicks(range: range, ticksCount: ticksCount);

      return ticks;
    }

    public int DecreaseTickCount(int ticksCount) {
      return MinorTicksProvider.DecreaseTickCount(ticksCount: ticksCount);
    }

    public int IncreaseTickCount(int ticksCount) {
      return MinorTicksProvider.IncreaseTickCount(ticksCount: ticksCount);
    }

    public ITicksProvider<double> MinorProvider => MinorTicksProvider.MinorProvider;

    public ITicksProvider<double> MajorProvider => owner;

    public event EventHandler Changed;

    #endregion
  }
}
