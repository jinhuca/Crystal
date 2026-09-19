namespace Crystal.Controls.PerformanceGraphs;

/// <summary>
/// A single-series performance graph fed one value at a time via <see cref="AddValue"/> - the feed
/// shape module view models drive on each sensor poll. Implemented by <see cref="PerformanceGraph"/>
/// and <see cref="PerformanceGraphLite"/>. <see cref="PerformanceGraphMultipleDS"/> deliberately does
/// not implement it: it plots several independent lines and is fed per-series through its
/// <see cref="DataSeries"/>, so there is no single stream to append to.
/// </summary>
public interface ISingleSeriesGraph : IPerformanceGraph {
  /// <summary>Appends a new sample to the series, dropping the oldest once capacity is exceeded.</summary>
  void AddValue(double value);
}
