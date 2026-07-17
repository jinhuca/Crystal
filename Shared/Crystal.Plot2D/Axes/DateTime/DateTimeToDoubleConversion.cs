using Crystal.Plot2D.Common.Auxiliary;

namespace Crystal.Plot2D.Axes;

internal sealed class DateTimeToDoubleConversion {
  public DateTimeToDoubleConversion(double min, System.DateTime minDate, double max, System.DateTime maxDate) {
    this.min = min;
    length = max - min;
    ticksMin = minDate.Ticks;
    ticksLength = maxDate.Ticks - ticksMin;
  }

  private readonly double min;
  private readonly double length;
  private readonly long ticksMin;
  private readonly long ticksLength;

  internal System.DateTime FromDouble(double d) {
    var ratio = (d - min) / length;
    var tick = (long)(ticksMin + ticksLength * ratio);

    tick = MathHelper.Clamp(value: tick, min: System.DateTime.MinValue.Ticks, max: System.DateTime.MaxValue.Ticks);

    return new System.DateTime(ticks: tick);
  }

  internal double ToDouble(System.DateTime dt) {
    var ratio = (dt.Ticks - ticksMin) / (double)ticksLength;
    return min + ratio * length;
  }
}
