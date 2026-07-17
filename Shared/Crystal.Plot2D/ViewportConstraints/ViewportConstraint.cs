using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;

namespace Crystal.Plot2D.ViewportConstraints;

/// <summary>
/// Represents a base class for all constraints that are being applied to viewport's visible rect.
/// </summary>
public abstract class ViewportConstraint {
  /// <summary>
  /// Applies the constraint.
  /// </summary>
  /// <param name="previousDataRect">Previous data rectangle.</param>
  /// <param name="proposedDataRect">Proposed data rectangle.</param>
  /// <param name="viewport">The viewport, to which current restriction is being applied.</param>
  /// <returns>New changed visible rectangle.</returns>
  public abstract DataRect Apply(DataRect previousDataRect, DataRect proposedDataRect, Viewport2D viewport);

  /// <summary>
  /// Raises the changed event.
  /// </summary>
  protected void RaiseChanged() {
    Changed.Raise(sender: this);
  }
  /// <summary>
  /// Occurs when constraint changes.
  /// Causes update of <see cref="Viewport"/>'s Visible property.
  /// </summary>
  public event EventHandler Changed;
}
