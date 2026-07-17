namespace Crystal.Plot2D.Axes.Numeric;

internal sealed class NumericConversion {
  private readonly double min;
  private readonly double length;
  private readonly double minValue;
  private readonly double valueLength;

  public NumericConversion(double min, double minValue, double max, double maxValue) {
    this.min = min;
    length = max - min;

    this.minValue = minValue;
    valueLength = maxValue - minValue;
  }

  public double FromDouble(double value) {
    var ratio = (value - min) / length;

    return minValue + ratio * valueLength;
  }

  public double ToDouble(double value) {
    var ratio = (value - minValue) / valueLength;

    return min + length * ratio;
  }
}
