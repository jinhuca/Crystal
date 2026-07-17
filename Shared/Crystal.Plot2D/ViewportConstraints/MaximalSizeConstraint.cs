using Crystal.Plot2D.Common;

namespace Crystal.Plot2D.ViewportConstraints;

/// <summary>
/// Represents a constraint, which limits the maximal size of <see cref="Viewport"/>'s Visible property.
/// </summary>
public class MaximalSizeConstraint : ViewportConstraint {
  /// <summary>
  /// Initializes a new instance of the <see cref="MaximalSizeConstraint"/> class.
  /// </summary>
  public MaximalSizeConstraint() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="MaximalSizeConstraint"/> class with the given maximal size of Viewport's Visible.
  /// </summary>
  /// <param name="maxSize">Maximal size of Viewport's Visible.</param>
  public MaximalSizeConstraint(double maxSize) {
    MaxSize = maxSize;
  }

  private double maxSize = 1000;
  /// <summary>
  /// Gets or sets the maximal size of Viewport's Visible.
  /// The default value is 1000.0.
  /// </summary>
  /// <value>The size of the max.</value>
  public double MaxSize {
    get => maxSize;
    set {
      if(maxSize != value) {
        maxSize = value;
        RaiseChanged();
      }
    }
  }

  /// <summary>
  /// Applies the specified old data rect.
  /// </summary>
  /// <param name="oldDataRect">The old data rect.</param>
  /// <param name="newDataRect">The new data rect.</param>
  /// <param name="viewport">The viewport.</param>
  /// <returns>DataRect</returns>
  public override DataRect Apply(DataRect oldDataRect, DataRect newDataRect, Viewport2D viewport) {
    var decreasing = newDataRect.Width < oldDataRect.Width || newDataRect.Height < oldDataRect.Height;
    if(!decreasing && (newDataRect.Width > maxSize || newDataRect.Height > maxSize)) {
      return oldDataRect;
    }

    return newDataRect;
  }
}
