using Crystal.Plot2D.Common;

namespace Crystal.Plot2D.Axes.TimeSpan;

public sealed class TimeSpanAxisControl : AxisControl<System.TimeSpan> {
  public TimeSpanAxisControl() {
    LabelProvider = new TimeSpanLabelProvider();
    TicksProvider = new TimeSpanTicksProvider();

    ConvertToDouble = time => time.Ticks;

    Range = new Range<System.TimeSpan>(min: new System.TimeSpan(), max: new System.TimeSpan(hours: 1, minutes: 0, seconds: 0));
  }
}
