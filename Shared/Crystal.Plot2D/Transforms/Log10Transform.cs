using Crystal.Plot2D.Common;
using System;
using System.Windows;

namespace Crystal.Plot2D.Transforms;

/// <summary>
/// Represents a logarithmic transform of both x- and y-values.
/// </summary>
public sealed class Log10Transform : DataTransform {
  /// <summary>
  /// Initializes a new instance of the <see cref="Log10Transform"/> class.
  /// </summary>
  public Log10Transform() { }

  /// <summary>
  /// Transforms the point in data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="pt">The point in data coordinates.</param>
  /// <returns>
  /// Transformed point in viewport coordinates.
  /// </returns>
  public override Point DataToViewport(Point pt) {
    var x = pt.X;
    var y = pt.Y;

    x = x < 0 ? double.MinValue : Math.Log10(d: x);
    y = y < 0 ? double.MinValue : Math.Log10(d: y);

    return new Point(x: x, y: y);
  }

  /// <summary>
  /// Transforms the point in viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="pt">The point in viewport coordinates.</param>
  /// <returns>Transformed point in data coordinates.</returns>
  public override Point ViewportToData(Point pt) => new(x: Math.Pow(x: 10, y: pt.X), y: Math.Pow(x: 10, y: pt.Y));

  /// <summary>
  /// Gets the data domain of this dataTransform.
  /// </summary>
  /// <value>The data domain of this dataTransform.</value>
  public override DataRect DataDomain => DataDomains.XYPositive;
}
