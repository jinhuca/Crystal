using System;
using System.Globalization;

namespace Crystal.Plot2D.Axes.Numeric;

public class CustomBaseNumericLabelProvider : LabelProvider<double> {
  private double customBase = 2;

  /// <summary>
  /// Gets or sets the custom base.
  /// </summary>
  /// <value>The custom base.</value>
  public double CustomBase {
    get => customBase;
    set {
      if(double.IsNaN(d: value)) {
        throw new ArgumentException(message: Strings.Exceptions.CustomBaseTicksProviderBaseIsNaN);
      }

      if(value <= 0) {
        throw new ArgumentOutOfRangeException(paramName: Strings.Exceptions.CustomBaseTicksProviderBaseIsTooSmall);
      }

      customBase = value;
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="CustomBaseNumericLabelProvider"/> class.
  /// </summary>
  public CustomBaseNumericLabelProvider() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="CustomBaseNumericLabelProvider"/> class.
  /// </summary>
  public CustomBaseNumericLabelProvider(double customBase)
    : this() {
    CustomBase = customBase;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="CustomBaseNumericLabelProvider"/> class.
  /// </summary>
  /// <param name="customBase">The custom base.</param>
  /// <param name="customBaseString">The custom base string.</param>
  public CustomBaseNumericLabelProvider(double customBase, string customBaseString)
    : this(customBase: customBase) {
    CustomBaseString = customBaseString;
  }

  private string customBaseString;

  /// <summary>
  /// Gets or sets the custom base string.
  /// </summary>
  /// <value>The custom base string.</value>
  public string CustomBaseString {
    get => customBaseString;
    set {
      if(customBaseString != value) {
        customBaseString = value;
        RaiseChanged();
      }
    }
  }

  protected override string GetStringCore(LabelTickInfo<double> tickInfo) {
    var value_ = tickInfo.Tick / customBase;

    var customBaseStr_ = customBaseString ?? customBase.ToString(provider: CultureInfo.InvariantCulture);
    string result_;
    if(Math.Abs(value_ - 1) < Constants.Constants.FloatComparisonTolerance) {
      result_ = customBaseStr_;
    }
    else if(Math.Abs(value_ - -1) < Constants.Constants.FloatComparisonTolerance) {
      result_ = "-" + customBaseStr_;
    }
    else {
      result_ = value_.ToString(provider: CultureInfo.InvariantCulture) + customBaseStr_;
    }

    return result_;
  }
}
