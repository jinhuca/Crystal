using Crystal.Plot2D.Common;

namespace Crystal.Plot2D.Axes.Numeric;

public sealed class NumericAxisControl : AxisControl<double> {
  public NumericAxisControl() {
    LabelProvider = new ExponentialLabelProvider();
    TicksProvider = new NumericTicksProvider();
    ConvertToDouble = d => d;
    Range = new Range<double>(min: 0, max: 10);
  }
}
