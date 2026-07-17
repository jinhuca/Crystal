namespace Crystal.Plot2D.Axes.Integer;

public class IntegerAxis : AxisBase<int> {
  protected IntegerAxis()
    : base(axisControl: new IntegerAxisControl(),
      convertFromDouble: d => (int)d,
      convertToDouble: i => (double)i) {
  }
}
