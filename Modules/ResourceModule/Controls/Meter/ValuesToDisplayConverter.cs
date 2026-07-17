using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ResourceModule.Controls.Meter;

public class ValuesToDisplayConverter : IMultiValueConverter {
  public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
    if (values == null || values.Any(static v => v == DependencyProperty.UnsetValue))
      return Binding.DoNothing;

    // (1) cast the passed values to doubles
    double valuePassed_, minValuePassed_, maxValuePassed_;

    try {
      valuePassed_ = System.Convert.ToDouble(values[0]);
      minValuePassed_ = System.Convert.ToDouble(values[1]);
      maxValuePassed_ = System.Convert.ToDouble(values[2]);
    }
    catch (InvalidCastException ice) {
      Debug.WriteLine(ice.Message);
      return string.Empty;
    }

    // (2) check the passed values validation
    if (!IsValidInput(valuePassed_, minValuePassed_, maxValuePassed_)) {
      return string.Empty;
    }

    // (3) cast the passed value to Unit defined
    if (Enum.TryParse<Unit>(values[3].ToString(), out Unit unit_)) {
      //Debug.WriteLine($"Parsed unit: {unit_}");
    }
    else {
      //Debug.WriteLine($"Failed to parse unit from value: {values[3]}");
      unit_ = Unit.None; // Default to None if parsing fails
    }

    // (4) convert the validated value to display
    double calculatedValue_ = valuePassed_;
    double result_ = 0;
    switch (unit_) {
      case Unit.Percent:
        calculatedValue_ = (valuePassed_ - minValuePassed_) / (maxValuePassed_ - minValuePassed_) * 100;
        result_ = Math.Round(calculatedValue_, 2);
        break;
      case Unit.Absolute:
        calculatedValue_ = valuePassed_ - minValuePassed_;
        result_ = Math.Round(calculatedValue_, 2);
        break;
      case Unit.Watts:
        calculatedValue_ = valuePassed_;
        result_ = Math.Round(calculatedValue_, 2);
        break;
      default:
        calculatedValue_ = valuePassed_;
        result_ = Math.Round(calculatedValue_, 2);
        break;
    }

    if (result_ is double num) {
      // Get the current culture's decimal separator
      string separator = culture.NumberFormat.NumberDecimalSeparator;

      // Format the number to a standard string
      string[] parts = num.ToString("F2", culture).Split(separator);

      // parameter "int" returns whole number, "dec" returns decimal part
      if (parameter?.ToString() == "dec") {
        return separator + parts[1];
      }
      return parts[0];
    }
    return string.Empty;
  }

  public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
    throw new NotImplementedException();
  }

  private bool IsValidInput(double value, double minValue, double maxValue) {
    return !(double.IsNaN(value) || double.IsNaN(minValue) || double.IsNaN(maxValue)
      || maxValue < minValue || value < minValue || value > maxValue);
  }
}
