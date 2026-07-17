using Crystal.Plot2D.Graphs;

namespace Crystal.Plot2D.Charts;

public sealed class LineAndMarker<T> {
  public LineGraph LineGraph { get; set; }
  public T MarkerGraph { get; set; }
}
