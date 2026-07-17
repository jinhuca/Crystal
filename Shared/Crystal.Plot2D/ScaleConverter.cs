using Crystal.Plot2D.Common;
using Crystal.Plot2D.Converters;
using System;
using System.Globalization;

namespace Crystal.Plot2D;

public sealed class ScaleConverter : GenericValueConverter<DataRect> {
  public void SetHorizontalTransform(double parentMin, double childMin, double parentMax, double childMax) {
    xScale = (childMax - childMin) / (parentMax - parentMin);
    xShift = childMin - parentMin;
  }

  public void SetVerticalTransform(double parentMin, double childMin, double parentMax, double childMax) {
    yScale = (childMax - childMin) / (parentMax - parentMin);
    yShift = childMin - parentMin;
  }

  private double xShift;
  private double xScale = 1;
  private double yShift;
  private double yScale = 1;

  protected override object ConvertCore(DataRect value) {
    var parentVisible = value;
    var xmin = parentVisible.XMin * xScale + xShift;
    var xmax = parentVisible.XMax * xScale + xShift;
    var ymin = parentVisible.YMin * yScale + yShift;
    var ymax = parentVisible.YMax * yScale + yShift;

    return DataRect.Create(xMin: xmin, yMin: ymin, xMax: xmax, yMax: ymax);
  }

  public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    if(targetType == typeof(DataRect)) {
      var childVisible = (DataRect)value;
      var xmin = (childVisible.XMin - xShift) / xScale;
      var xmax = (childVisible.XMax - xShift) / xScale;
      var ymin = (childVisible.YMin - yShift) / yScale;
      var ymax = (childVisible.YMax - yShift) / yScale;

      return DataRect.Create(xMin: xmin, yMin: ymin, xMax: xmax, yMax: ymax);
    }
    return base.ConvertBack(value: value, targetType: targetType, parameter: parameter, culture: culture);
  }
}
