using Crystal.Plot2D.Common;

namespace Crystal.Plot2D.Axes;

/// <summary>
/// AxisControl for DateTime axes.
/// </summary>
public sealed class DateTimeAxisControl : AxisControl<System.DateTime> {
  /// <summary>
  /// Initializes a new instance of the <see cref="DateTimeAxisControl"/> class.
  /// </summary>
  public DateTimeAxisControl() {
    LabelProvider = new DateTimeLabelProvider();
    TicksProvider = new DateTimeTicksProvider();
    MajorLabelProvider = new MajorDateTimeLabelProvider();

    ConvertToDouble = dt => dt.Ticks;

    Range = new Range<System.DateTime>(min: System.DateTime.Now, max: System.DateTime.Now.AddYears(value: 1));
  }
}
