using Crystal.Plot2D.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Crystal.Plot2D.Transforms;

public static class CoordinateTransformExtensions {
  #region Points

  /// <summary>
  /// Transforms point in data coordinates to screen coordinates.
  /// </summary>
  /// <param name="dataPoint">Point in data coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Point in screen coordinates</returns>
  public static Point DataToScreen(this Point dataPoint, CoordinateTransform transform) {
    return transform.DataToScreen(dataPoint: dataPoint);
  }

  /// <summary>
  /// Transforms point in screen coordinates to data coordinates.
  /// </summary>
  /// <param name="screenPoint">Point in screen coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Point in data coordinates</returns>
  public static Point ScreenToData(this Point screenPoint, CoordinateTransform transform) {
    return transform.ScreenToData(screenPoint: screenPoint);
  }

  /// <summary>
  /// Transforms point in screen coordinates to viewport coordinates.
  /// </summary>
  /// <param name="screenPoint">Point in screen coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Point in viewport coordinates</returns>
  public static Point ScreenToViewport(this Point screenPoint, CoordinateTransform transform) {
    return transform.ScreenToViewport(screenPoint: screenPoint);
  }

  /// <summary>
  /// Transforms point in viewport coordinates to screen coordinates.
  /// </summary>
  /// <param name="viewportPoint">Point in viewport coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Point in screen coordinates</returns>
  public static Point ViewportToScreen(this Point viewportPoint, CoordinateTransform transform) {
    return transform.ViewportToScreen(viewportPoint: viewportPoint);
  }

  /// <summary>
  /// Transforms point in viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="viewportPoint">Point in viewport coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Point in data coordinates</returns>
  private static Point ViewportToData(this Point viewportPoint, CoordinateTransform transform) {
    return transform.DataTransform.ViewportToData(pt: viewportPoint);
  }

  /// <summary>
  /// Transforms point in data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="dataPoint">Point in data coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Point in viewport coordinates</returns>
  public static Point DataToViewport(this Point dataPoint, CoordinateTransform transform) {
    return transform.DataTransform.DataToViewport(pt: dataPoint);
  }

  /// <summary>
  /// Transforms point in data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="dataPoint">Point in data coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Point in viewport coordinates</returns>
  private static Point DataToViewport(this Point dataPoint, DataTransform transform) {
    return transform.DataToViewport(pt: dataPoint);
  }

  #endregion

  #region Rects

  /// <summary>
  /// Transforms rectangle from screen coordinates to data coordinates.
  /// </summary>
  /// <param name="screenRect">Rectangle in screen coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Rectangle in data coordinates</returns>
  public static Rect ScreenToData(this Rect screenRect, CoordinateTransform transform) {
    var p1_ = screenRect.BottomLeft.ScreenToData(transform);
    var p2_ = screenRect.TopRight.ScreenToData(transform);

    return new Rect(point1: p1_, point2: p2_);
  }

  /// <summary>
  /// Transforms rectangle from data coordinates to screen coordinates.
  /// </summary>
  /// <param name="dataRect">Rectangle in data coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Rectangle in screen coordinates</returns>
  public static Rect DataToScreen(this DataRect dataRect, CoordinateTransform transform) {
    var p1_ = dataRect.XMaxYMax.DataToScreen(transform: transform);
    var p2_ = dataRect.XMinYMin.DataToScreen(transform: transform);

    return new Rect(point1: p1_, point2: p2_);
  }

  /// <summary>
  /// Transforms rectangle from screen coordinates to viewport coordinates.
  /// </summary>
  /// <param name="screenRect">Rectangle in screen coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Rectangle in viewport coordinates</returns>
  public static DataRect ScreenToViewport(this Rect screenRect, CoordinateTransform transform) {
    var p1_ = screenRect.BottomLeft.ScreenToViewport(transform);
    var p2_ = screenRect.TopRight.ScreenToViewport(transform);

    return new DataRect(point1: p1_, point2: p2_);
  }

  /// <summary>
  /// Transforms rectangle from viewport coordinates to screen coordinates.
  /// </summary>
  /// <param name="viewportRect">Rectangle in viewport coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Rectangle in screen coordinates</returns>
  public static Rect ViewportToScreen(this DataRect viewportRect, CoordinateTransform transform) {
    var p1_ = viewportRect.XMaxYMax.ViewportToScreen(transform);
    var p2_ = viewportRect.XMinYMin.ViewportToScreen(transform);

    return new Rect(point1: p1_, point2: p2_);
  }

  /// <summary>
  /// Transforms rectangle from viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="viewportRect">Rectangle in viewport coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Rectangle in data coordinates</returns>
  public static DataRect ViewportToData(this DataRect viewportRect, CoordinateTransform transform) {
    var p1_ = viewportRect.XMaxYMax.ViewportToData(transform);
    var p2_ = viewportRect.XMinYMin.ViewportToData(transform);

    return new DataRect(point1: p1_, point2: p2_);
  }

  /// <summary>
  /// Transforms rectangle from data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="dataRect">Rectangle in data coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Rectangle in viewport coordinates</returns>
  public static DataRect DataToViewport(this DataRect dataRect, CoordinateTransform transform) {
    return new DataRect(dataRect.XMaxYMax.DataToViewport(transform), dataRect.XMinYMin.DataToViewport(transform));
  }

  /// <summary>
  /// Transforms rectangle from viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="viewportRect">Rectangle in viewport coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Rectangle in data coordinates</returns>
  public static DataRect ViewportToData(this DataRect viewportRect, DataTransform transform) {
    var p1_ = transform.ViewportToData(pt: viewportRect.XMaxYMax);
    var p2_ = transform.ViewportToData(pt: viewportRect.XMinYMin);

    return new DataRect(point1: p1_, point2: p2_);
  }

  /// <summary>
  /// Transforms rectangle from data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="dataRect">Rectangle in data coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Rectangle in viewport coordinates</returns>
  public static DataRect DataToViewport(this DataRect dataRect, DataTransform transform) {
    var p1_ = transform.DataToViewport(pt: dataRect.XMinYMin);
    var p2_ = transform.DataToViewport(pt: dataRect.XMaxYMax);

    return new DataRect(point1: p1_, point2: p2_);
  }

  #endregion

  #region Collections

  public static IEnumerable<Point> ViewportToScreen(this IEnumerable<Point> viewportPoints, CoordinateTransform transform) {
    foreach(var point_ in viewportPoints) {
      yield return point_.ViewportToScreen(transform);
    }
  }

  public static IEnumerable<Point> DataToScreen(this IEnumerable<Point> dataPoints, CoordinateTransform transform) {
    foreach(var point_ in dataPoints) {
      yield return point_.DataToScreen(transform: transform);
    }
  }

  /// <summary>
  /// Transforms list of points from data coordinates to screen coordinates.
  /// </summary>
  /// <param name="dataPoints">Points in data coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Points in screen coordinates</returns>
  public static List<Point> DataToScreenAsList(this IEnumerable<Point> dataPoints, CoordinateTransform transform) {
    List<Point> res_;

    if(dataPoints is ICollection<Point> iCollection_) {
      res_ = new List<Point>(capacity: iCollection_.Count);
    }
    else {
      res_ = new List<Point>();
    }

    foreach(var point_ in dataPoints) {
      res_.Add(item: transform.DataToScreen(dataPoint: point_));
    }

    return res_;
  }

  /// <summary>
  /// Transforms list of points from data coordinates to screen coordinates.
  /// </summary>
  /// <param name="transform">Coordinate transform used to perform transformation</param>
  /// <param name="dataPoints">Points in data coordinates</param>
  /// <returns>List of points in screen coordinates</returns>
  [SuppressMessage(category: "Microsoft.Design", checkId: "CA1002:DoNotExposeGenericLists")]
  public static List<Point> DataToScreenAsList(this CoordinateTransform transform, IEnumerable<Point> dataPoints) {
    return dataPoints.DataToScreenAsList(transform);
  }

  /// <summary>
  /// Transforms list of points from data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="dataPoints">Points in data coordinates</param>
  /// <param name="transform">Data transform used to perform transformation</param>
  /// <returns>List of points in viewport coordinates</returns>
  public static IEnumerable<Point> DataToViewport(this IEnumerable<Point> dataPoints, DataTransform transform) {
    foreach(var pt_ in dataPoints) {
      yield return pt_.DataToViewport(transform);
    }
  }

  public static IEnumerable<Point> DataToViewport(this IEnumerable<Point> dataPoints, CoordinateTransform transform) {
    return dataPoints.DataToViewport(transform.DataTransform);
  }

  public static IEnumerable<Point> ScreenToViewport(this IEnumerable<Point> screenPoints, CoordinateTransform transform) {
    foreach(var pt_ in screenPoints) {
      yield return pt_.ScreenToViewport(transform);
    }
  }

  public static IEnumerable<Point> ScreenToData(this IEnumerable<Point> screenPoints, CoordinateTransform transform) {
    foreach(var pt_ in screenPoints) {
      yield return pt_.ScreenToData(transform);
    }
  }

  #endregion
}
