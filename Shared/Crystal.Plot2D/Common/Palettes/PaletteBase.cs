using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Windows.Media;

namespace Crystal.Plot2D.Common.Palettes;

/// <summary>
/// Represents a simple base class for a palette.
/// Contains an abstract method for creation of color and method to raise changed event.
/// </summary>
public abstract class PaletteBase : IPalette {
  #region IPalette Members

  /// <summary>
  /// Gets the color by interpolation coefficient.
  /// </summary>
  /// <param name="t">Interpolation coefficient, should belong to [0..1].</param>
  /// <returns>Color.</returns>
  public abstract Color GetColor(double t);

  protected void RaiseChanged() => Changed.Raise(sender: this);

  /// <summary>
  /// Occurs when palette changes.
  /// </summary>
  public event EventHandler Changed;

  #endregion
}
