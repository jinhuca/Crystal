using Crystal.Plot2D.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.Transforms;

/// <summary>
/// Base class for all data transforms.
/// Defines methods to transform point from data coordinate system to viewport coordinates and vice versa.
/// Derived class should be immutable; to perform any changes a new new instance with different parameters should be created.
/// </summary>
public abstract class DataTransform {
  /// <summary>
  /// Transforms the point in data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="pt">The point in data coordinates.</param>
  /// <returns>Transformed point in viewport coordinates.</returns>
  public abstract Point DataToViewport(Point pt);

  /// <summary>
  /// Transforms the point in viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="pt">The point in viewport coordinates.</param>
  /// <returns>Transformed point in data coordinates.</returns>
  public abstract Point ViewportToData(Point pt);

  /// <summary>
  /// Gets the data domain of this dataTransform.
  /// </summary>
  /// <value>The data domain of this dataTransform.</value>
  public virtual DataRect DataDomain => DefaultDomain;

  public static DataRect DefaultDomain { get; } = DataRect.Empty;
}

/// <summary>
/// Represents identity data transform, which applies no transformation.
/// is by default in CoordinateTransform.
/// </summary>
public sealed class IdentityTransform : DataTransform {
  /// <summary>
  /// Initializes a new instance of the <see cref="IdentityTransform"/> class.
  /// </summary>
  public IdentityTransform() { }

  /// <summary>
  /// Transforms the point in data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="pt">The point in data coordinates.</param>
  /// <returns></returns>
  public override Point DataToViewport(Point pt) => pt;

  /// <summary>
  /// Transforms the point in viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="pt">The point in viewport coordinates.</param>
  /// <returns></returns>
  public override Point ViewportToData(Point pt) => pt;
}

/// <summary>
/// Represents a logarithmic transform of y-values of points.
/// </summary>
public sealed class Log10YTransform : DataTransform {
  /// <summary>
  /// Initializes a new instance of the <see cref="Log10YTransform"/> class.
  /// </summary>
  public Log10YTransform() { }

  /// <summary>
  /// Transforms the point in data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="pt">The point in data coordinates.</param>
  /// <returns></returns>
  public override Point DataToViewport(Point pt) {
    var y = pt.Y;

    y = y < 0 ? double.MinValue : Math.Log10(d: y);

    return new Point(x: pt.X, y: y);
  }

  /// <summary>
  /// Transforms the point in viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="pt">The point in viewport coordinates.</param>
  /// <returns></returns>
  public override Point ViewportToData(Point pt) => new(x: pt.X, y: Math.Pow(x: 10, y: pt.Y));

  /// <summary>
  /// Gets the data domain of this dataTransform.
  /// </summary>
  /// <value>The data domain of this dataTransform.</value>
  public override DataRect DataDomain => DataDomains.YPositive;
}

/// <summary>
/// Represents a logarithmic transform of x-values of points.
/// </summary>
public sealed class Log10XTransform : DataTransform {
  /// <summary>
  /// Initializes a new instance of the <see cref="Log10XTransform"/> class.
  /// </summary>
  public Log10XTransform() { }

  /// <summary>
  /// Transforms the point in data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="pt">The point in data coordinates.</param>
  /// <returns></returns>
  public override Point DataToViewport(Point pt) {
    var x = pt.X;

    x = x < 0 ? double.MinValue : Math.Log10(d: x);

    return new Point(x: x, y: pt.Y);
  }

  /// <summary>
  /// Transforms the point in viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="pt">The point in viewport coordinates.</param>
  /// <returns></returns>
  public override Point ViewportToData(Point pt) => new(x: Math.Pow(x: 10, y: pt.X), y: pt.Y);

  /// <summary>
  /// Gets the data domain.
  /// </summary>
  /// <value>The data domain.</value>
  public override DataRect DataDomain => DataDomains.XPositive;
}

/// <summary>
/// Represents a mercator transform, used in maps.
/// Transforms y coordinates.
/// </summary>
public sealed class MercatorTransform : DataTransform {
  /// <summary>
  /// Initializes a new instance of the <see cref="MercatorTransform"/> class.
  /// </summary>
  public MercatorTransform() => CalcScale(maxLatitude: MaxLatitude);

  /// <summary>
  /// Initializes a new instance of the <see cref="MercatorTransform"/> class.
  /// </summary>
  /// <param name="maxLatitude">The maximal latitude.</param>
  public MercatorTransform(double maxLatitude) {
    MaxLatitude = maxLatitude;
    CalcScale(maxLatitude: maxLatitude);
  }

  private void CalcScale(double maxLatitude) {
    var maxLatDeg = maxLatitude;
    var maxLatRad = maxLatDeg * Math.PI / 180;
    Scale = maxLatDeg / Math.Log(d: Math.Tan(a: maxLatRad / 2 + Math.PI / 4));
  }
  /// <summary>
  /// Gets the scale.
  /// </summary>
  /// <value>The scale.</value>
  public double Scale { get; private set; }

  /// <summary>
  /// Gets the maximal latitude.
  /// </summary>
  /// <value>The max latitude.</value>
  public double MaxLatitude { get; } = 85;

  /// <summary>
  /// Transforms the point in data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="pt">The point in data coordinates.</param>
  /// <returns></returns>
  public override Point DataToViewport(Point pt) {
    var y = pt.Y;
    if(-MaxLatitude <= y && y <= MaxLatitude) {
      y = Scale * Math.Log(d: Math.Tan(a: Math.PI * (pt.Y + 90) / 360));
    }

    return new Point(x: pt.X, y: y);
  }

  /// <summary>
  /// Transforms the point in viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="pt">The point in viewport coordinates.</param>
  /// <returns></returns>
  public override Point ViewportToData(Point pt) {
    var y = pt.Y;
    if(-MaxLatitude <= y && y <= MaxLatitude) {
      var e = Math.Exp(d: y / Scale);
      y = 360 * Math.Atan(d: e) / Math.PI - 90;
    }

    return new Point(x: pt.X, y: y);
  }
}

/// <summary>
/// Represents transform from polar coordinate system to rectangular coordinate system.
/// </summary>
public sealed class PolarToRectTransform : DataTransform {
  /// <summary>
  /// Initializes a new instance of the <see cref="PolarToRectTransform"/> class.
  /// </summary>
  public PolarToRectTransform() { }

  /// <summary>
  /// Transforms the point in data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="pt">The point in data coordinates.</param>
  /// <returns></returns>
  public override Point DataToViewport(Point pt) {
    var r = pt.X;
    var phi = pt.Y;

    var x = r * Math.Cos(d: phi);
    var y = r * Math.Sin(a: phi);

    return new Point(x: x, y: y);
  }

  /// <summary>
  /// Transforms the point in viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="pt">The point in viewport coordinates.</param>
  /// <returns></returns>
  public override Point ViewportToData(Point pt) {
    var x = pt.X;
    var y = pt.Y;
    var r = Math.Sqrt(d: x * x + y * y);
    var phi = Math.Atan2(y: y, x: x);

    return new Point(x: r, y: phi);
  }
}

/// <summary>
/// Represents a data transform which applies rotation around specified center at specified angle.
/// </summary>
public sealed class RotateDataTransform : DataTransform {
  /// <summary>
  /// Initializes a new instance of the <see cref="RotateDataTransform"/> class.
  /// </summary>
  /// <param name="angleInRadians">The angle in radians.</param>
  public RotateDataTransform(double angleInRadians) {
    Center = new Point();
    Angle = angleInRadians;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="RotateDataTransform"/> class.
  /// </summary>
  /// <param name="angleInRadians">The angle in radians.</param>
  /// <param name="center">The center of rotation.</param>
  public RotateDataTransform(double angleInRadians, Point center) {
    Center = center;
    Angle = angleInRadians;
  }
  /// <summary>
  /// Gets the center of rotation.
  /// </summary>
  /// <value>The center.</value>
  public Point Center { get; }

  /// <summary>
  /// Gets the rotation angle.
  /// </summary>
  /// <value>The angle.</value>
  public double Angle { get; }

  /// <summary>
  /// Transforms the point in data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="pt">The point in data coordinates.</param>
  /// <returns></returns>
  public override Point DataToViewport(Point pt) => Transform(pt: pt, angle: Angle);

  /// <summary>
  /// Transforms the point in viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="pt">The point in viewport coordinates.</param>
  /// <returns></returns>
  public override Point ViewportToData(Point pt) => Transform(pt: pt, angle: -Angle);

  private Point Transform(Point pt, double angle) {
    var vec = pt - Center;
    var currAngle = Math.Atan2(y: vec.Y, x: vec.X);
    currAngle += angle;
    var rotatedVec = new Vector(x: Math.Cos(d: currAngle), y: Math.Sin(a: currAngle)) * vec.Length;
    return Center + rotatedVec;
  }
}

/// <summary>
/// Represents data transform performed by multiplication on given matrix.
/// </summary>
public sealed class MatrixDataTransform : DataTransform {
  /// <summary>
  /// Initializes a new instance of the <see cref="MatrixDataTransform"/> class.
  /// </summary>
  /// <param name="matrix">The transform matrix.</param>
  public MatrixDataTransform(Matrix matrix) {
    Matrix = matrix;
    InvertedMatrix = matrix;
    InvertedMatrix.Invert();
  }

  public Matrix Matrix { get; }

  public Matrix InvertedMatrix { get; }

  /// <summary>
  /// Transforms the point in data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="pt">The point in data coordinates.</param>
  /// <returns></returns>
  public override Point DataToViewport(Point pt) => Matrix.Transform(point: pt);

  /// <summary>
  /// Transforms the point in viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="pt">The point in viewport coordinates.</param>
  /// <returns></returns>
  public override Point ViewportToData(Point pt) => InvertedMatrix.Transform(point: pt);
}

/// <summary>
/// Represents a chain of transforms which are being applied consequently.
/// </summary>
public sealed class CompositeDataTransform : DataTransform {
  /// <summary>
  /// Initializes a new instance of the <see cref="CompositeDataTransform"/> class.
  /// </summary>
  /// <param name="transforms">The transforms.</param>
  public CompositeDataTransform(params DataTransform[] transforms) {
    ArgumentNullException.ThrowIfNull(transforms);

    foreach(var transform in transforms) {
      if(transform == null) {
        throw new ArgumentNullException(paramName: nameof(transforms), message: Strings.Exceptions.EachTransformShouldNotBeNull);
      }
    }

    Transforms = transforms;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="CompositeDataTransform"/> class.
  /// </summary>
  /// <param name="transforms">The transforms.</param>
  public CompositeDataTransform(IEnumerable<DataTransform> transforms) {
    Transforms = transforms ?? throw new ArgumentNullException(paramName: nameof(transforms));
  }

  /// <summary>
  /// Gets the transforms.
  /// </summary>
  /// <value>The transforms.</value>
  public IEnumerable<DataTransform> Transforms { get; }

  /// <summary>
  /// Transforms the point in data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="pt">The point in data coordinates.</param>
  /// <returns></returns>
  public override Point DataToViewport(Point pt) {
    foreach(var transform in Transforms) {
      pt = transform.DataToViewport(pt: pt);
    }

    return pt;
  }

  /// <summary>
  /// Transforms the point in viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="pt">The point in viewport coordinates.</param>
  /// <returns></returns>
  public override Point ViewportToData(Point pt) {
    foreach(var transform in Transforms.Reverse()) {
      pt = transform.ViewportToData(pt: pt);
    }

    return pt;
  }
}

/// <summary>
/// Represents a data transform, performed by given lambda function.
/// </summary>
public sealed class LambdaDataTransform : DataTransform {
  /// <summary>
  /// Initializes a new instance of the <see cref="DelegateDataTransform"/> class.
  /// </summary>
  /// <param name="dataToViewport">The data to viewport transform delegate.</param>
  /// <param name="viewportToData">The viewport to data transform delegate.</param>
  public LambdaDataTransform(Func<Point, Point> dataToViewport, Func<Point, Point> viewportToData) {
    DataToViewportFunc = dataToViewport ?? throw new ArgumentNullException(paramName: nameof(dataToViewport));
    ViewportToDataFunc = viewportToData ?? throw new ArgumentNullException(paramName: nameof(viewportToData));
  }
  /// <summary>
  /// Gets the data to viewport transform delegate.
  /// </summary>
  /// <value>The data to viewport func.</value>
  public Func<Point, Point> DataToViewportFunc { get; }

  /// <summary>
  /// Gets the viewport to data transform delegate.
  /// </summary>
  /// <value>The viewport to data func.</value>
  public Func<Point, Point> ViewportToDataFunc { get; }

  /// <summary>
  /// Transforms the point in data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="pt">The point in data coordinates.</param>
  /// <returns></returns>
  public override Point DataToViewport(Point pt) => DataToViewportFunc(arg: pt);

  /// <summary>
  /// Transforms the point in viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="pt">The point in viewport coordinates.</param>
  /// <returns></returns>
  public override Point ViewportToData(Point pt) => ViewportToDataFunc(arg: pt);
}

/// <summary>
/// Contains default data transforms.
/// </summary>
public static class DataTransforms {
  /// <summary>
  /// Gets the default identity data transform.
  /// </summary>
  /// <value>The identity data transform.</value>
  public static IdentityTransform Identity { get; } = new();
}
