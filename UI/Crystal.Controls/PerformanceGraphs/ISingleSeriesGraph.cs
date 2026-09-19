namespace Crystal.Controls.PerformanceGraphs;

/// <summary>
/// A single-series performance graph fed one value at a time via <see cref="AddValue"/> - the feed
/// shape module view models drive on each sensor poll. Implemented by <see cref="PerformanceGraph"/>.
/// A graph in <see cref="DisplayMode.MultipleLine"/> mode is instead fed per-series through its
/// <see cref="DataSeries"/>, so this single-stream append is the primary-series path.
/// </summary>
public interface ISingleSeriesGraph : IPerformanceGraph {
  /// <summary>Appends a new sample to the series, dropping the oldest once capacity is exceeded.</summary>
  void AddValue(double value);
}
