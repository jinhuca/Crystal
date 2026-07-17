using System.Windows;

namespace Crystal.Plot2D.Transforms;

/// <summary>
/// Represents a DataTransform that simply swaps points' coefficients from x to y and vice versa.
/// </summary>
public sealed class SwapTransform : DataTransform {
  /// <summary>
  /// Initializes a new instance of the <see cref="SwapTransform"/> class.
  /// </summary>
  public SwapTransform() { }

  /// <summary>
  /// Transforms the point in data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="pt">The point in data coordinates.</param>
  /// <returns>
  /// Transformed point in viewport coordinates.
  /// </returns>
  public override Point DataToViewport(Point pt) => new(x: pt.Y, y: pt.X);

  /// <summary>
  /// Transforms the point in viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="pt">The point in viewport coordinates.</param>
  /// <returns>Transformed point in data coordinates.</returns>
  public override Point ViewportToData(Point pt) => new(x: pt.Y, y: pt.X);
}
