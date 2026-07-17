using Crystal.Plot2D.Common;

namespace Crystal.Plot2D.Axes.Integer;

public sealed class IntegerAxisControl : AxisControl<int> {
  public IntegerAxisControl() {
    LabelProvider = new GenericLabelProvider<int>();
    TicksProvider = new IntegerTicksProvider();
    ConvertToDouble = i => (double)i;
    Range = new Range<int>(min: 0, max: 1);
  }
}
