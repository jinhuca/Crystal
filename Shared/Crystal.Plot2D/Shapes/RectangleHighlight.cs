using Crystal.Plot2D.Common;
using Crystal.Plot2D.Transforms;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.Shapes;

/// <summary>
/// Represents a rectangle with corners bound to viewport coordinates.
/// </summary>
public sealed class RectangleHighlight : ViewportShape {
  /// <summary>
  /// Initializes a new instance of the <see cref="RectangleHighlight"/> class.
  /// </summary>
  public RectangleHighlight() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="RectangleHighlight"/> class.
  /// </summary>
  /// <param name="bounds">The bounds.</param>
  public RectangleHighlight(Rect bounds) {
    Bounds = bounds;
  }

  private DataRect rect = DataRect.Empty;

  public DataRect Bounds {
    get => rect;
    set {
      if(rect != value) {
        rect = value;
        UpdateUIRepresentation();
      }
    }
  }

  protected override void UpdateUIRepresentationCore() {
    var transform = Plotter.Viewport.Transform;
    var p1 = rect.XMaxYMax.DataToScreen(transform: transform);
    var p2 = rect.XMinYMin.DataToScreen(transform: transform);
    rectGeometry.Rect = new Rect(point1: p1, point2: p2);
  }

  private readonly RectangleGeometry rectGeometry = new();
  protected override Geometry DefiningGeometry => rectGeometry;
}
