using Crystal.Plot2D.ViewportConstraints;
using System;

namespace Crystal.Plot2D.Axes;

public class VerticalDateTimeAxis : DateTimeAxis {
  public VerticalDateTimeAxis() {
    Placement = AxisPlacement.Left;
    Constraint = new DateTimeVerticalAxisConstraint();
  }

  protected override void ValidatePlacement(AxisPlacement newPlacement) {
    if(newPlacement == AxisPlacement.Bottom || newPlacement == AxisPlacement.Top) {
      throw new ArgumentException(message: Strings.Exceptions.VerticalAxisCannotBeHorizontal);
    }
  }
}
