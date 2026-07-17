using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace Crystal.Plot2D.Common;

/// <summary>
/// Describes a rectangle in viewport or data coordinates.
/// </summary>
[Serializable]
[ValueSerializer(valueSerializerType: typeof(DataRectSerializer))]
[TypeConverter(type: typeof(DataRectConverter))]
public struct DataRect : IEquatable<DataRect>, IFormattable {
  #region Ctors

  /// <summary>
  /// Initializes a new instance of the <see cref="DataRect"/> struct.
  /// </summary>
  /// <param name="rect">
  /// Source rect.
  /// </param>
  public DataRect(Rect rect) {
    xMin = rect.X;
    yMin = rect.Y;
    width = rect.Width;
    height = rect.Height;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="DataRect"/> struct.
  /// </summary>
  /// <param name="size">
  /// The size.
  /// </param>
  public DataRect(Size size) {
    if(size.IsEmpty) {
      this = emptyRect;
    }
    else {
      xMin = yMin = 0.0;
      width = size.Width;
      height = size.Height;
    }
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="DataRect"/> struct.
  /// </summary>
  /// <param name="location">The location.</param>
  /// <param name="size">The size.</param>
  public DataRect(Point location, Size size) {
    if(size.IsEmpty) {
      this = emptyRect;
    }
    else {
      xMin = location.X;
      yMin = location.Y;
      width = size.Width;
      height = size.Height;
    }
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="DataRect"/> struct.
  /// </summary>
  /// <param name="point1">The point1.</param>
  /// <param name="point2">The point2.</param>
  public DataRect(Point point1, Point point2) {
    xMin = Math.Min(val1: point1.X, val2: point2.X);
    yMin = Math.Min(val1: point1.Y, val2: point2.Y);
    width = Math.Max(val1: Math.Max(val1: point1.X, val2: point2.X) - xMin, val2: 0);
    height = Math.Max(val1: Math.Max(val1: point1.Y, val2: point2.Y) - yMin, val2: 0);
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="DataRect"/> struct.
  /// </summary>
  /// <param name="point">The point.</param>
  /// <param name="vector">The vector.</param>
  public DataRect(Point point, Vector vector) : this(point1: point, point2: point + vector) { }

  /// <summary>
  ///   Initializes a new instance of the <see cref="DataRect"/> struct.
  /// </summary>
  /// <param name="xMin">The minimal x.</param>
  /// <param name="yMin">The minimal y.</param>
  /// <param name="width">The width.</param>
  /// <param name="height">The height.</param>
  public DataRect(double xMin, double yMin, double width, double height) {
    if(width < 0 || height < 0) {
      throw new ArgumentException(message: Strings.Exceptions.WidthAndHeightCannotBeNegative);
    }

    this.xMin = xMin;
    this.yMin = yMin;
    this.width = width;
    this.height = height;
  }

  #endregion

  #region Static

  /// <summary>
  ///   Creates the DataRect from minimal and maximal 'x' and 'y' coordinates.
  /// </summary>
  /// <param name="xMin">The x min.</param>
  /// <param name="yMin">The y min.</param>
  /// <param name="xMax">The x max.</param>
  /// <param name="yMax">The y max.</param>
  /// <returns></returns>
  public static DataRect Create(double xMin, double yMin, double xMax, double yMax)
    => new(xMin: xMin, yMin: yMin, width: xMax - xMin, height: yMax - yMin);

  public static DataRect FromPoints(double x1, double y1, double x2, double y2)
    => new(point1: new Point(x: x1, y: y1), point2: new Point(x: x2, y: y2));

  public static DataRect FromCenterSize(Point center, double width, double height)
    => new(xMin: center.X - width / 2, yMin: center.Y - height / 2, width: width, height: height);

  public static DataRect FromCenterSize(Point center, Size size)
    => FromCenterSize(center: center, width: size.Width, height: size.Height);

  public static DataRect Intersect(DataRect rect1, DataRect rect2) {
    rect1.Intersect(rect: rect2);
    return rect1;
  }

  public static implicit operator DataRect(Rect rect) => new(rect: rect);

  #endregion

  public Rect ToRect() => new(x: xMin, y: yMin, width: width, height: height);

  public void Intersect(DataRect rect) {
    if(!IntersectsWith(rect: rect)) {
      this = Empty;
      return;
    }

    DataRect res_ = new();

    var x_ = Math.Max(val1: XMin, val2: rect.XMin);
    var y_ = Math.Max(val1: YMin, val2: rect.YMin);
    res_.width = Math.Max(val1: Math.Min(val1: XMax, val2: rect.XMax) - x_, val2: 0.0);
    res_.height = Math.Max(val1: Math.Min(val1: YMax, val2: rect.YMax) - y_, val2: 0.0);
    res_.xMin = x_;
    res_.yMin = y_;

    this = res_;
  }

  public bool IntersectsWith(DataRect rect) {
    if(IsEmpty || rect.IsEmpty) {
      return false;
    }

    return rect.XMin <= XMax && rect.XMax >= XMin && rect.YMax >= YMin && rect.YMin <= YMax;
  }

  private double width;
  private double height;

  /// <summary>
  ///   Gets a value indicating whether this instance is empty.
  /// </summary>
  /// <value>
  ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
  /// </value>
  public bool IsEmpty => width < 0;

  private double yMin;

  /// <summary>
  ///   Gets the bottom.
  /// </summary>
  /// <value>
  ///   The bottom.
  /// </value>
  public double YMin {
    get => yMin;
    set {
      if(IsEmpty) {
        throw new InvalidOperationException(message: Strings.Exceptions.CannotModifyEmptyDataRect);
      }

      yMin = value;
    }
  }

  /// <summary>
  ///   Gets the maximal y value.
  /// </summary>
  /// <value>
  ///   The top.
  /// </value>
  public double YMax => IsEmpty ? double.PositiveInfinity : yMin + height;

  private double xMin;

  /// <summary>
  ///   Gets the minimal x value.
  /// </summary>
  /// <value>
  ///   The left.
  /// </value>
  public double XMin {
    get => xMin;
    set {
      if(IsEmpty) {
        throw new InvalidOperationException(message: Strings.Exceptions.CannotModifyEmptyDataRect);
      }

      xMin = value;
    }
  }

  /// <summary>
  ///   Gets the maximal x value.
  /// </summary>
  /// <value>
  ///   The right.
  /// </value>
  public double XMax => IsEmpty ? double.PositiveInfinity : xMin + width;

  /// <summary>
  ///   Gets or sets the location.
  /// </summary>
  /// <value>
  ///   The location.
  /// </value>
  public Point Location {
    get => new(x: xMin, y: yMin);
    set {
      if(IsEmpty) {
        throw new InvalidOperationException(message: Strings.Exceptions.CannotModifyEmptyDataRect);
      }

      xMin = value.X;
      yMin = value.Y;
    }
  }

  public Point XMaxYMax => new(x: XMax, y: YMax);

  public Point XMinYMin => new(x: xMin, y: yMin);

  /// <summary>
  /// Gets or sets the size.
  /// </summary>
  /// <value>The size.</value>
  public Size Size {
    get => IsEmpty ? Size.Empty : new Size(width: width, height: height);
    set {
      if(value.IsEmpty) {
        this = emptyRect;
      }
      else {
        if(IsEmpty) {
          throw new InvalidOperationException(message: Strings.Exceptions.CannotModifyEmptyDataRect);
        }

        width = value.Width;
        height = value.Height;
      }
    }
  }

  public double Width {
    get => width;
    set {
      if(IsEmpty) {
        throw new InvalidOperationException(message: Strings.Exceptions.CannotModifyEmptyDataRect);
      }

      if(value < 0) {
        throw new ArgumentOutOfRangeException(paramName: Strings.Exceptions.DataRectSizeCannotBeNegative);
      }

      width = value;
    }
  }

  public double Height {
    get => height;
    set {
      if(IsEmpty) {
        throw new InvalidOperationException(message: Strings.Exceptions.CannotModifyEmptyDataRect);
      }

      if(value < 0) {
        throw new ArgumentOutOfRangeException(paramName: Strings.Exceptions.DataRectSizeCannotBeNegative);
      }

      height = value;
    }
  }

  private static readonly DataRect emptyRect = CreateEmptyRect();

  public static DataRect Empty => emptyRect;

  private static DataRect CreateEmptyRect() {
    DataRect rect_ = new() {
      xMin = double.PositiveInfinity,
      yMin = double.PositiveInfinity,
      width = double.NegativeInfinity,
      height = double.NegativeInfinity
    };
    return rect_;
  }

  public static DataRect Infinite { get; } = new(xMin: double.MinValue / 2, yMin: double.MinValue / 2, width: double.MaxValue, height: double.MaxValue);

  #region Object overrides

  /// <summary>
  /// Indicates whether this instance and a specified object are equal.
  /// </summary>
  /// <param name="obj">Another object to compare to.</param>
  /// <returns>
  /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
  /// </returns>
  public override bool Equals(object obj) {
    if(obj == null) {
      return false;
    }

    if(!(obj is DataRect)) {
      return false;
    }

    var other_ = (DataRect)obj;

    return Equals(other: other_);
  }

  /// <summary>
  /// Returns the hash code for this instance.
  /// </summary>
  /// <returns>
  /// A 32-bit signed integer that is the hash code for this instance.
  /// </returns>
  public override int GetHashCode() {
    if(IsEmpty) {
      return 0;
    }

    return xMin.GetHashCode() ^
        width.GetHashCode() ^
        yMin.GetHashCode() ^
        height.GetHashCode();
  }

  /// <summary>
  /// Returns the fully qualified type name of this instance.
  /// </summary>
  /// <returns>
  /// A <see cref="T:System.String"/> containing a fully qualified type name.
  /// </returns>
  public override string ToString() {
    if(IsEmpty) {
      return "Empty";
    }

    return $"({xMin};{yMin}) -> {width}*{height}";
  }

  /// <summary>
  /// Implements the operator ==.
  /// </summary>
  /// <param name="rect1">The rect1.</param>
  /// <param name="rect2">The rect2.</param>
  /// <returns>The result of the operator.</returns>
  public static bool operator ==(DataRect rect1, DataRect rect2) {
    return rect1.Equals(other: rect2);
  }

  /// <summary>
  /// Implements the operator !=.
  /// </summary>
  /// <param name="rect1">The rect1.</param>
  /// <param name="rect2">The rect2.</param>
  /// <returns>The result of the operator.</returns>
  public static bool operator !=(DataRect rect1, DataRect rect2) {
    return !rect1.Equals(other: rect2);
  }

  public static bool EqualEps(DataRect rect1, DataRect rect2, double eps) {
    var width_ = Math.Min(val1: rect1.Width, val2: rect2.Width);
    var height_ = Math.Min(val1: rect1.Height, val2: rect2.Height);
    return Math.Abs(value: rect1.XMin - rect2.XMin) < width_ * eps &&
         Math.Abs(value: rect1.XMax - rect2.XMax) < width_ * eps &&
         Math.Abs(value: rect1.YMin - rect2.YMin) < height_ * eps &&
         Math.Abs(value: rect1.YMax - rect2.YMax) < height_ * eps;
  }

  #endregion

  #region IEquatable<DataRect> Members

  /// <summary>
  /// Indicates whether the current object is equal to another object of the same type.
  /// </summary>
  /// <param name="other">An object to compare with this object.</param>
  /// <returns>
  /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
  /// </returns>
  public bool Equals(DataRect other) {
    if(IsEmpty) {
      return other.IsEmpty;
    }

    return Math.Abs(xMin - other.xMin) < Constants.Constants.FloatComparisonTolerance &&
           Math.Abs(width - other.width) < Constants.Constants.FloatComparisonTolerance &&
           Math.Abs(yMin - other.yMin) < Constants.Constants.FloatComparisonTolerance &&
           Math.Abs(height - other.height) < Constants.Constants.FloatComparisonTolerance;
  }

  #endregion

  /// <summary>
  /// Determines whether this DataRect contains point with specified coordinates.
  /// </summary>
  /// <param name="x">The x coordinate of point.</param>
  /// <param name="y">The y coordinate of point.</param>
  /// <returns>
  /// 	<c>true</c> if contains point with specified coordinates; otherwise, <c>false</c>.
  /// </returns>
  public bool Contains(double x, double y) {
    if(IsEmpty) {
      return false;
    }

    return x >= xMin &&
      x <= XMax &&
      y >= yMin &&
      y <= YMax;
  }

  public bool Contains(Point point) {
    return Contains(x: point.X, y: point.Y);
  }

  public bool Contains(DataRect rect) {
    if(IsEmpty || rect.IsEmpty) {
      return false;
    }

    return
      xMin <= rect.xMin &&
      yMin <= rect.yMin &&
      XMax >= rect.XMax &&
      YMax >= rect.YMax;
  }

  public void Offset(Vector offsetVector) {
    if(IsEmpty) {
      throw new InvalidOperationException(message: Strings.Exceptions.CannotModifyEmptyDataRect);
    }

    xMin += offsetVector.X;
    yMin += offsetVector.Y;
  }

  public void Offset(double offsetX, double offsetY) {
    if(IsEmpty) {
      throw new InvalidOperationException(message: Strings.Exceptions.CannotModifyEmptyDataRect);
    }

    xMin += offsetX;
    yMin += offsetY;
  }

  public static DataRect Offset(DataRect rect, double offsetX, double offsetY) {
    rect.Offset(offsetX: offsetX, offsetY: offsetY);
    return rect;
  }

  internal void UnionFinite(DataRect rect) {
    if(!rect.IsEmpty) {
      if(rect.xMin.IsInfinite()) {
        rect.xMin = 0;
      }

      if(rect.yMin.IsInfinite()) {
        rect.yMin = 0;
      }

      if(rect.width.IsInfinite()) {
        rect.width = 0;
      }

      if(rect.height.IsInfinite()) {
        rect.height = 0;
      }
    }

    Union(rect: rect);
  }

  public void Union(DataRect rect) {
    if(IsEmpty) {
      this = rect;
    }
    else if(!rect.IsEmpty) {
      var minX_ = Math.Min(val1: xMin, val2: rect.xMin);
      var minY_ = Math.Min(val1: yMin, val2: rect.yMin);

      if(double.IsPositiveInfinity(rect.width) || double.IsPositiveInfinity(width)) {
        width = double.PositiveInfinity;
      }
      else {
        var maxX_ = Math.Max(val1: XMax, val2: rect.XMax);
        width = Math.Max(val1: maxX_ - minX_, val2: 0.0);
      }

      if(double.IsPositiveInfinity(rect.height) || double.IsPositiveInfinity(height)) {
        height = double.PositiveInfinity;
      }
      else {
        var maxY_ = Math.Max(val1: YMax, val2: rect.YMax);
        height = Math.Max(val1: maxY_ - minY_, val2: 0.0);
      }

      xMin = minX_;
      yMin = minY_;
    }
  }

  public void Union(Point point) {
    Union(rect: new DataRect(point1: point, point2: point));
  }

  public static DataRect Union(DataRect rect, Point point) {
    rect.Union(point: point);

    return rect;
  }

  public static DataRect Union(DataRect rect1, DataRect rect2) {
    rect1.Union(rect: rect2);

    return rect1;
  }

  internal string ConvertToString(string format, IFormatProvider provider) {
    if(IsEmpty) {
      return "Empty";
    }

    var listSeparator_ = TokenizerHelper.GetNumericListSeparator(provider: provider);
    return string.Format(provider: provider, format: "{1:" + format + "}{0}{2:" + format + "}{0}{3:" + format + "}{0}{4:" + format + "}",
      args: new object[] { listSeparator_, xMin, yMin, width, height });
  }

  /// <summary>
  /// Parses the specified string as a DataRect.
  /// </summary>
  /// <remarks>
  /// There are three possible string patterns, that are recognized as string representation of DataRect:
  /// 1) Literal string "Empty" - represents an DataRect.Empty rect;
  /// 2) String in format "d,d,d,d", where d is a floating-point number with '.' as decimal separator - is considered as a string 
  /// of "XMin,YMin,Width,Height";
  /// 3) String in format "d,d d,d", where d is a floating-point number with '.' as decimal separator - is considered as a string
  /// of "XMin,YMin XMax,YMax".
  /// </remarks>
  /// <param name="source">The source.</param>
  /// <returns>DataRect, parsed from the given input string.</returns>
  public static DataRect Parse(string source) {
    DataRect rect_;
    IFormatProvider cultureInfo_ = CultureInfo.GetCultureInfo(name: "en-us");

    var values_ = source.Split(separator: ',');
    if(source == "Empty") {
      rect_ = Empty;
    }
    else {
      // format X,Y,Width,Height
      if(values_.Length == 4) {
        rect_ = new DataRect(
          xMin: Convert.ToDouble(value: values_[0], provider: cultureInfo_),
          yMin: Convert.ToDouble(value: values_[1], provider: cultureInfo_),
          width: Convert.ToDouble(value: values_[2], provider: cultureInfo_),
          height: Convert.ToDouble(value: values_[3], provider: cultureInfo_)
          );
      }
      else {
        // format XMin, YMin - XMax, YMax
        values_ = source.Split(separator: new[] { ',', ' ' }, options: StringSplitOptions.RemoveEmptyEntries);
        rect_ = Create(
          xMin: Convert.ToDouble(value: values_[0], provider: cultureInfo_),
          yMin: Convert.ToDouble(value: values_[1], provider: cultureInfo_),
          xMax: Convert.ToDouble(value: values_[2], provider: cultureInfo_),
          yMax: Convert.ToDouble(value: values_[3], provider: cultureInfo_)
          );
      }
    }

    return rect_;
  }

  #region IFormattable Members

  string IFormattable.ToString(string format, IFormatProvider formatProvider) {
    return ConvertToString(format: format, provider: formatProvider);
  }

  #endregion
}
