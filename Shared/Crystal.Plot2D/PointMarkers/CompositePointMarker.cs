using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.PointMarkers;

/// <summary>
///   Composite point markers renders a specified set of markers at every point of graph.
/// </summary>
public sealed class CompositePointMarker : PointMarker {
  public CompositePointMarker() { }

  public CompositePointMarker(params PointMarker[] markers) {
    ArgumentNullException.ThrowIfNull(markers);

    foreach(var m in markers) {
      Markers.Add(item: m);
    }
  }

  public CompositePointMarker(IEnumerable<PointMarker> markers) {
    ArgumentNullException.ThrowIfNull(markers);

    foreach(var m in markers) {
      Markers.Add(item: m);
    }
  }
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Content)]
  public Collection<PointMarker> Markers { get; } = new();

  public override void Render(DrawingContext dc, Point screenPoint) {
    var enumerator = GetLocalValueEnumerator();
    foreach(var marker in Markers) {
      enumerator.Reset();
      while(enumerator.MoveNext()) {
        marker.SetValue(dp: enumerator.Current.Property, value: enumerator.Current.Value);
      }

      marker.Render(dc: dc, screenPoint: screenPoint);
    }
  }
}
