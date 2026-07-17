using Crystal.Plot2D.Common;
using System;

namespace Crystal.Plot2D.ViewportConstraints;

/// <summary>
/// Represents a restriction in which actual visible rect's proportions depends on 
/// actual output rect's proportions.
/// </summary>
public sealed class PhysicalProportionsConstraint : ViewportConstraint {
  /// <summary>
  /// Initializes a new instance of the <see cref="PhysicalProportionsConstraint"/> class.
  /// </summary>
  public PhysicalProportionsConstraint() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="PhysicalProportionsConstraint"/> class with the given proportion ratio.
  /// </summary>
  /// <param name="proportionRatio">The proportion ratio.</param>
  public PhysicalProportionsConstraint(double proportionRatio) {
    ProportionRatio = proportionRatio;
  }

  private double proportionRatio = 1.0;
  /// <summary>Gets or sets the proportion ratio (width / height). Default value is 1.0.</summary>
  /// <value>The proportion ratio.</value>
  public double ProportionRatio {
    get => proportionRatio;
    set {
      if(proportionRatio != value) {
        proportionRatio = value;
        RaiseChanged();
      }
    }
  }

  /// <summary>
  /// Applies the restriction.
  /// </summary>
  /// <param name="previousDataRect">Previous data rectangle.</param>
  /// <param name="proposedDataRect">Proposed data rectangle.</param>
  /// <param name="viewport">The viewport, to which current restriction is being applied.</param>
  /// <returns>New changed visible rectangle.</returns>
  public override DataRect Apply(DataRect previousDataRect, DataRect proposedDataRect, Viewport2D viewport) {
    var output = viewport.Output;
    if(output.Height == 0 || output.Width == 0) {
      return proposedDataRect;
    }

    var newRatio = proposedDataRect.Width * output.Height /
                   (proposedDataRect.Height * output.Width);

    // Don't modify rect if new ratio differs only slightly 
    if(Math.Abs(value: newRatio - proportionRatio) < 1e-3) {
      return proposedDataRect;
    }

    // Try to keep visible rect's square constant
    double width = proposedDataRect.Width, height = proposedDataRect.Height;
    var square = proposedDataRect.Width * proposedDataRect.Height;
    if(square > 0) {
      width = Math.Sqrt(d: proportionRatio * output.Width * square / output.Height);
      height = Math.Sqrt(d: output.Height * square / (proportionRatio * output.Width));
    }

    // Finally ensure we have correct aspect ratio
    var delta = (proportionRatio * height * output.Width - width * output.Height) /
                (output.Height + proportionRatio * output.Width);
    width += delta;
    height -= delta;

    var x0 = (proposedDataRect.XMax + proposedDataRect.XMin) / 2;
    var y0 = (proposedDataRect.YMax + proposedDataRect.YMin) / 2;

    return new DataRect {
      XMin = x0 - width / 2,
      Width = width,
      YMin = y0 - height / 2,
      Height = height
    };
  }
}
