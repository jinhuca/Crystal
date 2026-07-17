using System;

namespace Crystal.Plot2D.Axes.Integer;

public class HorizontalIntegerAxis : IntegerAxis {
  public HorizontalIntegerAxis() {
    Placement = AxisPlacement.Bottom;
  }

  protected override void ValidatePlacement(AxisPlacement newPlacement) {
    if(newPlacement == AxisPlacement.Left || newPlacement == AxisPlacement.Right) {
      throw new ArgumentException(message: Strings.Exceptions.HorizontalAxisCannotBeVertical);
    }
  }
}
