using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;

namespace Crystal.Plot2D.ViewportConstraints;

public sealed class ProportionsConstraint : ViewportConstraint {
  private double widthToHeightRatio = 1;

  public double WidthToHeightRatio {
    get => widthToHeightRatio;
    set {
      if(!(Math.Abs(widthToHeightRatio - value) > Constants.Constants.FloatComparisonTolerance)) return;
      widthToHeightRatio = value;
      RaiseChanged();
    }
  }

  public override DataRect Apply(DataRect oldDataRect, DataRect newDataRect, Viewport2D viewport) {
    var ratio_ = newDataRect.Width / newDataRect.Height;
    var coeff_ = Math.Sqrt(d: ratio_);

    var newWidth_ = newDataRect.Width / coeff_;
    var newHeight_ = newDataRect.Height * coeff_;

    var center_ = newDataRect.GetCenter();
    var res_ = DataRect.FromCenterSize(center: center_, width: newWidth_, height: newHeight_);
    return res_;
  }
}
