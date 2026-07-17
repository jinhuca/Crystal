using System.Windows;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class DataRectExtensions {
  internal static bool IsNaN(this DataRect rect) => !rect.IsEmpty && (rect.XMin.IsNaN() || rect.YMin.IsNaN() || rect.XMax.IsNaN() || rect.YMax.IsNaN());

  public static Point GetCenter(this DataRect rect) => new(x: rect.XMin + rect.Width * 0.5, y: rect.YMin + rect.Height * 0.5);

  public static DataRect Zoom(this DataRect rect, Point to, double ratio) => CoordinateUtilities.RectZoom(rect: rect, zoomCenter: to, ratio: ratio);

  public static DataRect ZoomOutFromCenter(this DataRect rect, double ratio) => CoordinateUtilities.RectZoom(rect: rect, zoomCenter: rect.GetCenter(), ratio: ratio);

  public static DataRect ZoomInToCenter(this DataRect rect, double ratio) => CoordinateUtilities.RectZoom(rect: rect, zoomCenter: rect.GetCenter(), ratio: 1 / ratio);

  public static DataRect ZoomX(this DataRect rect, Point to, double ratio) => CoordinateUtilities.RectZoomX(rect: rect, zoomCenter: to, ratio: ratio);

  public static DataRect ZoomY(this DataRect rect, Point to, double ratio) => CoordinateUtilities.RectZoomY(rect: rect, zoomCenter: to, ratio: ratio);

  public static double GetSquare(this DataRect rect) {
    if(rect.IsEmpty) {
      return 0;
    }

    return rect.Width * rect.Height;
  }

  /// <summary>
  ///   Determines whether one DataRect is close to another DataRect.
  /// </summary>
  /// <param name="rect1">The rect1.</param>
  /// <param name="rect2">The rect2.</param>
  /// <param name="difference">The difference.</param>
  /// <returns>
  /// 	<c>true</c> if [is close to] [the specified rect1]; otherwise, <c>false</c>.
  /// </returns>
  public static bool IsCloseTo(this DataRect rect1, DataRect rect2, double difference) {
    var intersection = DataRect.Intersect(rect1: rect1, rect2: rect2);
    var square1 = rect1.GetSquare();
    var square2 = rect2.GetSquare();
    var intersectionSquare = intersection.GetSquare();

    var areClose = MathHelper.AreClose(d1: square1, d2: intersectionSquare, diffRatio: difference) &&
                   MathHelper.AreClose(d1: square2, d2: intersectionSquare, diffRatio: difference);
    return areClose;
  }
}