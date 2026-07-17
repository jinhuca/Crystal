using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.ComponentModel;
using System.Windows;

namespace Crystal.Plot2D.ViewportConstraints;

/// <summary>
/// Represents a viewport constraint which modifies x coordinates of result visible rect to be adjacent to the right border of initial rect and have a fixed given width.
/// Probably is better to add to FitToViewConstraints collection of <see cref="Viewport"/>.
/// </summary>
public class FollowWidthConstraint : ViewportConstraint {
  /// <summary>
  /// Initializes a new instance of the <see cref="FollowWidthConstraint"/> class.
  /// </summary>
  public FollowWidthConstraint() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="FollowWidthConstraint"/> class with the given width.
  /// </summary>
  /// <param name="width">The width.</param>
  public FollowWidthConstraint(double width) => Width = width;

  private double width = 1;
  /// <summary>
  /// Gets or sets the width of result visible rectangle.
  /// Default value is 1.0.
  /// </summary>
  /// <value>The width.</value>
  [DefaultValue(value: 1.0)]
  public double Width {
    get => width;
    set {
      if(width != value) {
        width = value;
        RaiseChanged();
      }
    }
  }

  /// <summary>
  /// Applies the constraint.
  /// </summary>
  /// <param name="previousDataRect">Previous data rectangle.</param>
  /// <param name="proposedDataRect">Proposed data rectangle.</param>
  /// <param name="viewport">The viewport, to which current constraint is being applied.</param>
  /// <returns>New changed visible rectangle.</returns>
  public override DataRect Apply(DataRect previousDataRect, DataRect proposedDataRect, Viewport2D viewport) {
    if(proposedDataRect.IsEmpty) {
      return proposedDataRect;
    }

    var followWidth = proposedDataRect.Width;
    if(!viewport.UnitedContentBounds.IsEmpty) {
      followWidth = Math.Min(val1: width, val2: viewport.UnitedContentBounds.Width);
    }
    if(followWidth.IsInfinite()) {
      followWidth = width;
    }

    return new Rect(x: proposedDataRect.XMin + proposedDataRect.Width - followWidth, y: proposedDataRect.YMin, width: followWidth, height: proposedDataRect.Height);
  }
}
