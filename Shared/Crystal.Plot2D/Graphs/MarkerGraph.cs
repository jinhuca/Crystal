using Crystal.Plot2D.Common;
using Crystal.Plot2D.PointMarkers;
using System.Windows.Media;

namespace Crystal.Plot2D.Graphs;

public class MarkerGraph<T> : PointsGraphBase where T : PointMarker {
  public MarkerGraph(T marker) {
    Marker = marker;
  }

  private T Marker { get; }

  protected override void OnRenderCore(DrawingContext dc, RenderState state) {
    if(DataSource == null || Marker == null) {
      return;
    }
  }
}
