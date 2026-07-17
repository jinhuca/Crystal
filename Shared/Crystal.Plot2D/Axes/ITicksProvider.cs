using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Diagnostics;

namespace Crystal.Plot2D.Axes;

/// <summary>
/// Contains information about one minor tick - its value (relative size) and its tick.
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerDisplay(value: "{Value} @ {Tick}")]
public readonly struct MinorTickInfo<T> {
  internal MinorTickInfo(double value, T tick) {
    this.value = value;
    this.tick = tick;
  }

  private readonly double value;
  private readonly T tick;

  public double Value => value;
  public T Tick => tick;

  public override string ToString() {
    return string.Format(format: "{0} @ {1}", arg0: value, arg1: tick);
  }
}

/// <summary>
/// Contains data for all generated ticks.
/// Used by TicksLabelProvider.
/// </summary>
/// <typeparam name="T">Type of axis tick.</typeparam>
public interface ITicksInfo<out T> {
  /// <summary>
  /// Gets the array of axis ticks.
  /// </summary>
  /// <value>The ticks.</value>
  T[] Ticks { get; }
  /// <summary>
  /// Gets the tick sizes.
  /// </summary>
  /// <value>The tick sizes.</value>
  double[] TickSizes { get; }
  /// <summary>
  /// Gets the additional information, added to ticks info and specifying range's features.
  /// </summary>
  /// <value>The info.</value>
  object Info { get; }
}

internal sealed class TicksInfo<T> : ITicksInfo<T> {
  private T[] ticks = { };
  /// <summary>
  /// Gets the array of axis ticks.
  /// </summary>
  /// <value>The ticks.</value>
  public T[] Ticks {
    get => ticks;
    internal set => ticks = value;
  }

  private double[] tickSizes = { };
  /// <summary>
  /// Gets the tick sizes.
  /// </summary>
  /// <value>The tick sizes.</value>
  public double[] TickSizes {
    get {
      if(tickSizes.Length != ticks.Length) {
        tickSizes = ArrayExtensions.CreateArray(length: ticks.Length, defaultValue: 1.0);
      }

      return tickSizes;
    }
    internal set => tickSizes = value;
  }

  private object info;
  /// <summary>
  /// Gets the additional information, added to ticks info and specifying range's features.
  /// </summary>
  /// <value>The info.</value>
  public object Info {
    get => info;
    internal set => info = value;
  }

  private static readonly TicksInfo<T> empty = new() { info = null, ticks = Array.Empty<T>(), tickSizes = Array.Empty<double>() };
  internal static TicksInfo<T> Empty => empty;
}

/// <summary>
///	Base interface for ticks generator.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ITicksProvider<T> {
  /// <summary>
  /// Generates ticks for given range and preferred ticks count.
  /// </summary>
  /// <param name="range">The range.</param>
  /// <param name="ticksCount">The ticks count.</param>
  /// <returns></returns>
  ITicksInfo<T> GetTicks(Range<T> range, int ticksCount);

  /// <summary>
  /// Decreases the tick count.
  /// Returned value should be later passed as ticksCount parameter to GetTicks method.
  /// </summary>
  /// <param name="ticksCount">The ticks count.</param>
  /// <returns>Decreased ticks count.</returns>
  int DecreaseTickCount(int ticksCount);

  /// <summary>
  /// Increases the tick count.
  /// Returned value should be later passed as ticksCount parameter to GetTicks method.
  /// </summary>
  /// <param name="ticksCount">The ticks count.</param>
  /// <returns>Increased ticks count.</returns>
  int IncreaseTickCount(int ticksCount);

  /// <summary>
  /// Gets the minor ticks provider, used to generate ticks between each two adjacent ticks.
  /// </summary>
  /// <value>The minor provider. If there is no minor provider available, returns null.</value>
  ITicksProvider<T> MinorProvider { get; }
  /// <summary>
  /// Gets the major provider, used to generate major ticks - for example, years for common ticks as months.
  /// </summary>
  /// <value>The major provider. If there is no major provider available, returns null.</value>
  ITicksProvider<T> MajorProvider { get; }

  /// <summary>
  /// Occurs when properties of ticks provider changed.
  /// Notifies axis to rebuild its view.
  /// </summary>
  event EventHandler Changed;
}
