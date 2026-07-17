using System;

namespace Crystal.Plot2D.Axes.Integer;

public class VerticalIntegerAxis : IntegerAxis {
  public VerticalIntegerAxis() {
    Placement = AxisPlacement.Left;
  }

  protected override void ValidatePlacement(AxisPlacement newPlacement) {
    if(newPlacement == AxisPlacement.Bottom || newPlacement == AxisPlacement.Top) {
      throw new ArgumentException(message: Strings.Exceptions.VerticalAxisCannotBeHorizontal);
    }
  }
}
