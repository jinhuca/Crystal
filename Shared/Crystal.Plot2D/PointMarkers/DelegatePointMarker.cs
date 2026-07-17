using System;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.PointMarkers;

/// <summary>
///   Invokes specified delegate for rendering custom marker at every point of graph.
/// </summary>
public sealed class DelegatePointMarker : PointMarker {
  public MarkerRenderHandler RenderCallback { get; }

  public DelegatePointMarker() {
  }

  public DelegatePointMarker(MarkerRenderHandler renderCallback) {
    ArgumentNullException.ThrowIfNull(renderCallback);
    RenderCallback = renderCallback;
  }

  public override void Render(DrawingContext dc, Point screenPoint) {
    RenderCallback(dc: dc, screenPoint: screenPoint);
  }
}
