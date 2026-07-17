using System.Diagnostics;
using System.Windows;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class RectExtensions {
  public static Point GetCenter(this Rect rect) => new(x: rect.Left + rect.Width * 0.5, y: rect.Top + rect.Height * 0.5);

  public static Rect FromCenterSize(Point center, Size size) => FromCenterSize(center: center, width: size.Width, height: size.Height);

  public static Rect FromCenterSize(Point center, double width, double height) => new(x: center.X - width / 2, y: center.Y - height / 2, width: width, height: height);

  public static Rect Zoom(this Rect rect, Point to, double ratio) => CoordinateUtilities.RectZoom(rect: rect, zoomCenter: to, ratio: ratio);

  public static Rect ZoomOutFromCenter(this Rect rect, double ratio) => CoordinateUtilities.RectZoom(rect: rect, zoomCenter: rect.GetCenter(), ratio: ratio);

  public static Rect ZoomInToCenter(this Rect rect, double ratio) => CoordinateUtilities.RectZoom(rect: rect, zoomCenter: rect.GetCenter(), ratio: 1 / ratio);

  public static Int32Rect ToInt32Rect(this Rect rect) => new(x: (int)rect.X, y: (int)rect.Y, width: (int)rect.Width, height: (int)rect.Height);

  [DebuggerStepThrough]
  public static DataRect ToDataRect(this Rect rect) => new(rect: rect);

  internal static bool IsNaN(this Rect rect) => !rect.IsEmpty && (rect.X.IsNaN() || rect.Y.IsNaN() || rect.Width.IsNaN() || rect.Height.IsNaN());
}
