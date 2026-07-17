using Crystal.Plot2D.Common;
using System;
using System.Diagnostics;
using System.Windows;

namespace Crystal.Plot2D.Transforms;

/// <summary>
/// A central class in 2d coordinate transformation in Plotter2D.
/// Provides methods to transform point from one coordinate system to another.
/// Should be immutable.
/// </summary>
public sealed class CoordinateTransform {
  private CoordinateTransform(DataRect visibleRect, Rect screenRect) {
    VisibleRect = visibleRect;
    ScreenRect1 = screenRect;

    rxToScreen = screenRect.Width / visibleRect.Width;
    ryToScreen = screenRect.Height / visibleRect.Height;
    cxToScreen = visibleRect.XMin * rxToScreen - screenRect.Left;
    cyToScreen = screenRect.Height + screenRect.Top + visibleRect.YMin * ryToScreen;

    rxToData = visibleRect.Width / screenRect.Width;
    ryToData = visibleRect.Height / screenRect.Height;
    cxToData = screenRect.Left * rxToData - visibleRect.XMin;
    cyToData = visibleRect.Height + visibleRect.YMin + screenRect.Top * ryToData;
  }

  #region Coeffs

  private readonly double rxToScreen;
  private readonly double ryToScreen;
  private readonly double cxToScreen;
  private readonly double cyToScreen;

  private readonly double rxToData;
  private readonly double ryToData;
  private readonly double cxToData;
  private readonly double cyToData;
  #endregion

  #region Creation methods

  internal static CoordinateTransform FromRects(DataRect visibleRect, Rect screenRect) {
    CoordinateTransform result = new(visibleRect: visibleRect, screenRect: screenRect);
    return result;
  }

  internal CoordinateTransform WithRects(DataRect visibleRect, Rect screenRect) {
    CoordinateTransform copy = new(visibleRect: visibleRect, screenRect: screenRect);
    copy.dataTransform = dataTransform;
    return copy;
  }

  /// <summary>
  /// Creates a new instance of CoordinateTransform with the given data transform.
  /// </summary>
  /// <param name="dataTransform">The data transform.</param>
  /// <returns></returns>
  public CoordinateTransform WithDataTransform(DataTransform dataTransform) {
    return new CoordinateTransform(visibleRect: VisibleRect, screenRect: ScreenRect1) {
      dataTransform = dataTransform ?? throw new ArgumentNullException(paramName: nameof(dataTransform))
    };
  }

  internal CoordinateTransform WithScreenOffset(double x, double y) {
    var screenCopy = ScreenRect1;
    screenCopy.Offset(offsetX: x, offsetY: y);
    CoordinateTransform copy = new(visibleRect: VisibleRect, screenRect: screenCopy);
    return copy;
  }

  internal static CoordinateTransform CreateDefault() => new(visibleRect: new Rect(x: 0, y: 0, width: 1, height: 1), screenRect: new Rect(x: 0, y: 0, width: 1, height: 1));

  #endregion

  #region Transform methods

  /// <summary>
  /// Transforms point from data coordinates to screen.
  /// </summary>
  /// <param name="dataPoint">The point in data coordinates.</param>
  /// <returns></returns>
  public Point DataToScreen(Point dataPoint) {
    var viewportPoint = dataTransform.DataToViewport(pt: dataPoint);

    Point screenPoint = new(x: viewportPoint.X * rxToScreen - cxToScreen,
        y: cyToScreen - viewportPoint.Y * ryToScreen);

    return screenPoint;
  }

  /// <summary>
  /// Transforms point from screen coordinates to data coordinates.
  /// </summary>
  /// <param name="screenPoint">The point in screen coordinates.</param>
  /// <returns></returns>
  public Point ScreenToData(Point screenPoint) {
    Point viewportPoint = new(x: screenPoint.X * rxToData - cxToData,
        y: cyToData - screenPoint.Y * ryToData);

    var dataPoint = dataTransform.ViewportToData(pt: viewportPoint);

    return dataPoint;
  }

  /// <summary>
  /// Transforms point from viewport coordinates to screen coordinates.
  /// </summary>
  /// <param name="viewportPoint">The point in viewport coordinates.</param>
  /// <returns></returns>
  public Point ViewportToScreen(Point viewportPoint) {
    Point screenPoint = new(x: viewportPoint.X * rxToScreen - cxToScreen,
        y: cyToScreen - viewportPoint.Y * ryToScreen);

    return screenPoint;
  }

  /// <summary>
  /// Transforms point from screen coordinates to viewport coordinates.
  /// </summary>
  /// <param name="screenPoint">The point in screen coordinates.</param>
  /// <returns></returns>
  public Point ScreenToViewport(Point screenPoint) {
    Point viewportPoint = new(x: screenPoint.X * rxToData - cxToData,
        y: cyToData - screenPoint.Y * ryToData);

    return viewportPoint;
  }

  #endregion

  [DebuggerBrowsable(state: DebuggerBrowsableState.Never)]
  private DataRect visibleRect;
  /// <summary>
  /// Gets the viewport rectangle.
  /// </summary>
  /// <value>The viewport rect.</value>
  public DataRect ViewportRect => VisibleRect;

  [DebuggerBrowsable(state: DebuggerBrowsableState.Never)]
  private Rect screenRect;

  /// <summary>
  /// Gets the screen rectangle.
  /// </summary>
  /// <value>The screen rect.</value>
  public Rect ScreenRect => ScreenRect1;

  [DebuggerBrowsable(state: DebuggerBrowsableState.Never)]
  private DataTransform dataTransform = DataTransforms.Identity;
  /// <summary>
  /// Gets the data transform.
  /// </summary>
  /// <value>The data transform.</value>
  public DataTransform DataTransform => dataTransform;

  public DataRect VisibleRect { get => visibleRect; set => visibleRect = value; }
  public Rect ScreenRect1 { get => screenRect; set => screenRect = value; }
}
