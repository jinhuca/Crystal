using Crystal.Plot2D.Axes;
using System;


namespace Crystal.Plot2D;

public class TimeChartPlotter : Plotter {
  public TimeChartPlotter() {
    MainHorizontalAxis = new HorizontalDateTimeAxis();
  }

  public void SetHorizontalAxisMapping(Func<double, DateTime> fromDouble, Func<DateTime, double> toDouble) {
    var axis = (HorizontalDateTimeAxis)MainHorizontalAxis;
    axis.ConvertFromDouble = fromDouble ?? throw new ArgumentNullException(paramName: nameof(fromDouble));
    axis.ConvertToDouble = toDouble ?? throw new ArgumentNullException(paramName: nameof(toDouble));
  }

  public void SetHorizontalAxisMapping(double min, DateTime minDate, double max, DateTime maxDate) {
    var axis = (HorizontalDateTimeAxis)MainHorizontalAxis;
    axis.SetConversion(min: min, minValue: minDate, max: max, maxValue: maxDate);
  }
}
