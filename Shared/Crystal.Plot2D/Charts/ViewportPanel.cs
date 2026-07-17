using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;
using System.Windows;

namespace Crystal.Plot2D.Charts;

/// <inheritdoc />
/// <summary>
///   Represents a panel on which elements are arranged in coordinates in viewport space.
/// </summary>
public partial class ViewportPanel : IndividualArrangePanel {
  static ViewportPanel() {
    var thisType_ = typeof(ViewportPanel);
    PlotterBase.PlotterProperty.OverrideMetadata(forType: thisType_, typeMetadata: new FrameworkPropertyMetadata { PropertyChangedCallback = OnPlotterChanged });
  }

  private static void OnPlotterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var panel_ = (ViewportPanel)d;
    panel_.OnPlotterChanged(currPlotter: (PlotterBase)e.NewValue);
  }

  private void OnPlotterChanged(PlotterBase currPlotter) {
    if(currPlotter != null) {
      var plotter2d_ = (PlotterBase)currPlotter;
      viewport = plotter2d_.Viewport;
    }
    else {
      viewport = null;
    }
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="ViewportRectPanel"/> class.
  /// </summary>
  protected ViewportPanel() {
  }

  #region Panel methods override

  protected internal override void OnChildAdded(FrameworkElement child) {
    InvalidatePosition(child: child);
  }

  protected virtual void InvalidatePosition(FrameworkElement child) {
    if(viewport == null) {
      return;
    }

    var transform_ = GetTransform(size: availableSize);

    var elementSize_ = GetElementSize(child: child, availableSize: AvailableSize, transform: transform_);
    child.Measure(availableSize: elementSize_);

    var bounds_ = GetElementScreenBounds(transform: transform_, child: child);
    if(!bounds_.IsNaN()) {
      child.Arrange(finalRect: bounds_);
    }
  }

  private Size availableSize;
  protected Size AvailableSize {
    get => availableSize;
    set => availableSize = value;
  }

  protected override Size MeasureOverride(Size availableSize) {
    this.availableSize = availableSize;

    var transform_ = GetTransform(size: availableSize);

    foreach(FrameworkElement child_ in InternalChildren) {
      if(child_ != null) {
        var elementSize_ = GetElementSize(child: child_, availableSize: availableSize, transform: transform_);
        child_.Measure(availableSize: elementSize_);
      }
    }

    if(availableSize.Width.IsInfinite()) {
      availableSize.Width = 0;
    }
    if(availableSize.Height.IsInfinite()) {
      availableSize.Height = 0;
    }
    return availableSize;
  }

  protected Size GetElementSize(FrameworkElement child, Size availableSize, CoordinateTransform transform) {
    Size res_;
    var ownViewportBounds_ = GetViewportBounds(obj: child);

    if(!ownViewportBounds_.IsEmpty) {
      res_ = ownViewportBounds_.ViewportToScreen(transform: transform).Size;
    }
    else {
      var viewportWidth_ = GetViewportWidth(obj: child);
      var viewportHeight_ = GetViewportHeight(obj: child);
      var hasViewportWidth_ = viewportWidth_.IsNotNaN();
      var hasViewportHeight_ = viewportHeight_.IsNotNaN();
      var minScreenWidth_ = GetMinScreenWidth(obj: child);
      var hasMinScreenWidth_ = minScreenWidth_.IsNotNaN();

      var selfWidth_ = child.Width.IsNotNaN() ? child.Width : availableSize.Width;
      var width_ = hasViewportWidth_ ? viewportWidth_ : selfWidth_;
      var selfHeight_ = child.Height.IsNotNaN() ? child.Height : availableSize.Height;
      var height_ = hasViewportHeight_ ? viewportHeight_ : selfHeight_;

      if(width_ < 0) {
        width_ = 0;
      }

      if(height_ < 0) {
        height_ = 0;
      }

      DataRect bounds_ = new(size: new Size(width: width_, height: height_));
      var screenBounds_ = bounds_.ViewportToScreen(transform: transform);

      res_ = new Size(width: hasViewportWidth_ ? screenBounds_.Width : selfWidth_,
        height: hasViewportHeight_ ? screenBounds_.Height : selfHeight_);

      if(hasMinScreenWidth_ && res_.Width < minScreenWidth_) {
        res_.Width = minScreenWidth_;
      }
    }

    if(res_.Width.IsNaN()) {
      res_.Width = 0;
    }
    if(res_.Height.IsNaN()) {
      res_.Height = 0;
    }

    return res_;
  }

  protected Rect GetElementScreenBounds(CoordinateTransform transform, UIElement child) {
    var screenBounds_ = GetElementScreenBoundsCore(transform: transform, child: child);
    var viewportBounds_ = screenBounds_.ScreenToViewport(transform: transform);
    var prevViewportBounds_ = GetActualViewportBounds(obj: child);
    SetPrevActualViewportBounds(obj: child, value: prevViewportBounds_);
    SetActualViewportBounds(obj: child, value: viewportBounds_);

    return screenBounds_;
  }

  protected Rect GetElementScreenBoundsCore(CoordinateTransform transform, UIElement child) {
    Rect bounds_ = new(x: 0, y: 0, width: 1, height: 1);

    var ownViewportBounds_ = GetViewportBounds(obj: child);
    if(!ownViewportBounds_.IsEmpty) {
      bounds_ = ownViewportBounds_.ViewportToScreen(transform: transform);
    }
    else {
      var viewportX_ = GetX(obj: child);
      var viewportY_ = GetY(obj: child);

      if(viewportX_.IsNaN() || viewportY_.IsNaN()) {
        //Debug.WriteLine("ViewportRectPanel: Position is not set!");
        return bounds_;
      }

      var viewportWidth_ = GetViewportWidth(obj: child);
      if(viewportWidth_ < 0) {
        viewportWidth_ = 0;
      }

      var viewportHeight_ = GetViewportHeight(obj: child);
      if(viewportHeight_ < 0) {
        viewportHeight_ = 0;
      }

      var hasViewportWidth_ = viewportWidth_.IsNotNaN();
      var hasViewportHeight_ = viewportHeight_.IsNotNaN();

      DataRect r_ = new(size: new Size(width: hasViewportWidth_ ? viewportWidth_ : child.DesiredSize.Width,
        height: hasViewportHeight_ ? viewportHeight_ : child.DesiredSize.Height));

      r_ = r_.ViewportToScreen(transform: transform);

      var screenWidth_ = hasViewportWidth_ ? r_.Width : child.DesiredSize.Width;
      var screenHeight_ = hasViewportHeight_ ? r_.Height : child.DesiredSize.Height;

      var minScreenWidth_ = GetMinScreenWidth(obj: child);
      var hasMinScreenWidth_ = minScreenWidth_.IsNotNaN();

      if(hasViewportWidth_ && screenWidth_ < minScreenWidth_) {
        screenWidth_ = minScreenWidth_;
      }

      var location_ = new Point(x: viewportX_, y: viewportY_).ViewportToScreen(transform: transform);
      var screenX_ = location_.X;
      var screenY_ = location_.Y;

      var horizontalAlignment_ = GetViewportHorizontalAlignment(obj: child);
      switch(horizontalAlignment_) {
        case HorizontalAlignment.Stretch:
        case HorizontalAlignment.Center:
          screenX_ -= screenWidth_ / 2;
          break;
        case HorizontalAlignment.Left:
          break;
        case HorizontalAlignment.Right:
          screenX_ -= screenWidth_;
          break;
      }

      var verticalAlignment_ = GetViewportVerticalAlignment(obj: child);
      switch(verticalAlignment_) {
        case VerticalAlignment.Bottom:
          screenY_ -= screenHeight_;
          break;
        case VerticalAlignment.Center:
        case VerticalAlignment.Stretch:
          screenY_ -= screenHeight_ / 2;
          break;
        case VerticalAlignment.Top:
          break;
      }

      bounds_ = new Rect(x: screenX_, y: screenY_, width: screenWidth_, height: screenHeight_);
    }

    // applying screen offset
    var screenOffsetX_ = GetScreenOffsetX(obj: child);
    if(screenOffsetX_.IsNaN()) {
      screenOffsetX_ = 0;
    }
    var screenOffsetY_ = GetScreenOffsetY(obj: child);
    if(screenOffsetY_.IsNaN()) {
      screenOffsetY_ = 0;
    }

    Vector screenOffset_ = new(x: screenOffsetX_, y: screenOffsetY_);
    bounds_.Offset(offsetVector: screenOffset_);
    return bounds_;
  }

  protected override Size ArrangeOverride(Size finalSize) {
    var transform_ = GetTransform(size: finalSize);

    foreach(UIElement child_ in InternalChildren) {
      if(child_ != null) {
        var bounds_ = GetElementScreenBounds(transform: transform_, child: child_);
        if(!bounds_.IsNaN()) {
          child_.Arrange(finalRect: bounds_);
        }
      }
    }

    return finalSize;
  }

  private CoordinateTransform GetTransform(Size size)
    => viewport.Transform.WithRects(visibleRect: GetViewportBounds(obj: this), screenRect: new Rect(size: size));

  #endregion

  private Viewport2D viewport;
}