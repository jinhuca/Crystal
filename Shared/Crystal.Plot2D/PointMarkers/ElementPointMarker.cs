using System.Windows;

namespace Crystal.Plot2D.PointMarkers;

/// <summary>
///   Provides elements that represent markers along the graph.
/// </summary>
public abstract class ElementPointMarker : DependencyObject {
  /// <summary>
  ///   Creates marker element at specified point.
  /// </summary>
  /// <returns>
  ///   UIElement representing marker.
  /// </returns>
  internal abstract UIElement CreateMarker();

  internal abstract void SetMarkerProperties(UIElement marker);

  /// <summary>
  ///   Moves specified marker so its center is located at specified screen point.
  /// </summary>
  /// <param name="marker">
  ///   UIElement created using CreateMarker.
  /// </param>
  /// <param name="screenPoint">
  ///   Point to center element around.
  /// </param>
  internal abstract void SetPosition(UIElement marker, Point screenPoint);
}
