using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ResourceModule.Controls.Meter;

public class ValuesToAngleConverter : IMultiValueConverter {
  public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
    // (1) cast the passed values to double
    double valuePassed_, minValuePassed_, maxValuePassed_;
    try {
      valuePassed_ = System.Convert.ToDouble(values[0]);
      minValuePassed_ = System.Convert.ToDouble(values[1]);
      maxValuePassed_ = System.Convert.ToDouble(values[2]);
    }
    catch (Exception ex) {
      Debug.WriteLine(ex);
      return DependencyProperty.UnsetValue;
    }

    // (2) check the values validation
    if (!IsValidInput(valuePassed_, minValuePassed_, maxValuePassed_)) {
      return DependencyProperty.UnsetValue;
    }

    // (3) Normalize and clamp to [PredefinedMinAngle, PredefinedMaxAngle]
    var mappedValue = MapRange(valuePassed_, minValuePassed_, maxValuePassed_, PredefinedMinAngle, PredefinedMaxAngle);

    // (4) calculate the mapped value to angle
    //var cacluatedValueAngle_1 = mappedValue * (PredefinedMaxAngle - PredefinedMinAngle) + PredefinedMinAngle;

    //var cacluatedValueAngle_ = (valuePassed_ - minValuePassed_) * (PredefinedMaxAngle - PredefinedMinAngle) 
    //  / (maxValuePassed_ - minValuePassed_) + PredefinedMinAngle;

    //Debug.WriteLine(mappedValue.ToString());
    return mappedValue;
  }

  public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
    throw new NotImplementedException();
  }

  private bool IsValidInput(double value, double minValue, double maxValue) {
    return !(double.IsNaN(value) || double.IsNaN(minValue) || double.IsNaN(maxValue)
      || maxValue < minValue || value < minValue || value > maxValue);
  }

  private static double MapRange(double value, double fromMin, double fromMax, double toMin, double toMax) {
    return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
  }

  public const double PredefinedMinAngle = -120.0;
  public const double PredefinedMaxAngle = 120.0;
}