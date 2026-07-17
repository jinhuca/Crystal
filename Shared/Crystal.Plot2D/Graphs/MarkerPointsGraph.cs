using Crystal.Plot2D.Common;
using Crystal.Plot2D.DataSources.OneDimensional;
using Crystal.Plot2D.PointMarkers;
using Crystal.Plot2D.Transforms;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.Graphs;

/// <summary>
/// Class represents a series of markers.
/// </summary>
public sealed class MarkerPointsGraph : PointsGraphBase {
  /// <summary>
  /// Initializes a new instance of the <see cref="MarkerPointsGraph"/> class.
  /// </summary>
  public MarkerPointsGraph() {
    ManualTranslate = true;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="MarkerPointsGraph"/> class.
  /// </summary>
  /// <param name="dataSource">
  /// The data source.
  /// </param>
  public MarkerPointsGraph(IPointDataSource dataSource) : this() {
    DataSource = dataSource;
  }

  protected override void OnVisibleChanged(DataRect newRect, DataRect oldRect) {
    base.OnVisibleChanged(newRect: newRect, oldRect: oldRect);
    InvalidateVisual();
  }

  public PointMarker Marker {
    get => (PointMarker)GetValue(dp: MarkerProperty);
    set => SetValue(dp: MarkerProperty, value: value);
  }

  public static readonly DependencyProperty MarkerProperty = DependencyProperty.Register(
    name: nameof(Marker),
    propertyType: typeof(PointMarker),
    ownerType: typeof(MarkerPointsGraph),
    typeMetadata: new FrameworkPropertyMetadata { DefaultValue = null, AffectsRender = true });

  protected override void OnRenderCore(DrawingContext dc, RenderState state) {
    if(DataSource == null || Marker == null) {
      return;
    }

    var transform = Plotter.Viewport.Transform;
    var bounds = DataRect.Empty;
    using var enumerator = DataSource.GetEnumerator(context: GetContext());
    Point point = new();
    while(enumerator.MoveNext()) {
      enumerator.GetCurrent(p: ref point);
      enumerator.ApplyMappings(target: Marker);

      //Point screenPoint = point.Transform(state.Visible, state.Output);
      var screenPoint = point.DataToScreen(transform: transform);

      bounds = DataRect.Union(rect: bounds, point: point);
      Marker.Render(dc: dc, screenPoint: screenPoint);
    }

    Viewport2D.SetContentBounds(obj: this, value: bounds);
  }
}
