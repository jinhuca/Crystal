using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Crystal.Plot2D.Charts;

public class ViewportHostPanel : ViewportPanel, IPlotterElement {
  static ViewportHostPanel() {
    EventManager.RegisterClassHandler(classType: typeof(ViewportHostPanel), routedEvent: PlotterBase.PlotterChangedEvent, handler: new PlotterChangedEventHandler(OnPlotterChanged));
  }

  public ViewportHostPanel() {
    RenderTransform = translateTransform;

#if false
			// for debug purposes
			Width = 100;
			Height = 100;
			Background = Brushes.PaleGreen.MakeTransparent(0.2);
#endif
  }

  private static void OnPlotterChanged(object sender, PlotterChangedEventArgs e) {
    var owner_ = (ViewportHostPanel)sender;

    if(owner_.plotter != null && e.PreviousPlotter != null) {
      owner_.viewport.PropertyChanged -= owner_.Viewport_PropertyChanged;
      owner_.viewport = null;
      owner_.plotter = null;
    }

    if(owner_.plotter == null && e.CurrentPlotter != null) {
      owner_.plotter = (PlotterBase)e.CurrentPlotter;
      owner_.viewport = owner_.plotter.Viewport;
      owner_.viewport.PropertyChanged += owner_.Viewport_PropertyChanged;
    }
  }

  private readonly TranslateTransform translateTransform = new();

  private readonly Canvas hostingCanvas = new();
  internal Canvas HostingCanvas => hostingCanvas;

  #region IPlotterElement Members

  private PlotterBase plotter;
  protected PlotterBase Plotter => plotter;

  private Viewport2D viewport;
  public virtual void OnPlotterAttached(PlotterBase plotter) {
    this.plotter = (PlotterBase)plotter;
    viewport = this.plotter.Viewport;
    if(!IsMarkersHost) {
      plotter.CentralGrid.Children.Add(element: hostingCanvas);
    }
    if(Parent == null) {
      hostingCanvas.Children.Add(element: this);
    }
    this.plotter.Viewport.PropertyChanged += Viewport_PropertyChanged;
  }

  public virtual void OnPlotterDetaching(PlotterBase plotter) {
    this.plotter.Viewport.PropertyChanged -= Viewport_PropertyChanged;
    if(!IsMarkersHost) {
      plotter.CentralGrid.Children.Remove(element: hostingCanvas);
    }
    hostingCanvas.Children.Remove(element: this);
    this.plotter = null;
  }

  PlotterBase IPlotterElement.Plotter => plotter;

  protected override void OnChildDesiredSizeChanged(UIElement child) {
    InvalidatePosition(child: (FrameworkElement)child);
  }

  private Vector prevVisualOffset;
  private bool sizeChanged = true;
  private DataRect visibleWhileCreation;
  protected virtual void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e) {
    if(e.PropertyName == "Visible") {
      var visible_ = (DataRect)e.NewValue;
      var prevVisible_ = (DataRect)e.OldValue;

      if(prevVisible_.Size.EqualsApproximately(size2: visible_.Size) && !sizeChanged) {
        var transform_ = viewport.Transform;
        var prevLocation_ = prevVisible_.Location.ViewportToScreen(transform: transform_);
        var location_ = visible_.Location.ViewportToScreen(transform: transform_);

        var offset_ = prevLocation_ - location_;
        translateTransform.X += offset_.X;
        translateTransform.Y += offset_.Y;
      }
      else {
        visibleWhileCreation = visible_;
        translateTransform.X = 0;
        translateTransform.Y = 0;
        InvalidateMeasure();
      }

      sizeChanged = false;
    }
    else if(e.PropertyName == "Output") {
      sizeChanged = true;
      translateTransform.X = 0;
      translateTransform.Y = 0;
      InvalidateMeasure();
    }

    prevVisualOffset = VisualOffset;
  }

  #endregion

  protected internal override void OnChildAdded(FrameworkElement child) {
    if(plotter == null) {
      return;
    }

    InvalidatePosition(child: child);
    //Dispatcher.BeginInvoke(((Action)(()=> InvalidatePosition(child))), DispatcherPriority.ApplicationIdle);
  }

  private DataRect overallViewportBounds = DataRect.Empty;
  internal DataRect OverallViewportBounds {
    get => overallViewportBounds;
    set => overallViewportBounds = value;
  }

  private BoundsUnionMode boundsUnionMode;
  internal BoundsUnionMode BoundsUnionMode {
    get => boundsUnionMode;
    set => boundsUnionMode = value;
  }

  protected override void InvalidatePosition(FrameworkElement child) {
    invalidatePositionCalls++;

    if(viewport == null) {
      return;
    }

    if(child.Visibility != Visibility.Visible) {
      return;
    }

    var transform_ = viewport.Transform.WithScreenOffset(x: -translateTransform.X, y: -translateTransform.Y);
    var elementSize_ = GetElementSize(child: child, availableSize: AvailableSize, transform: transform_);
    child.Measure(availableSize: elementSize_);

    var bounds_ = GetElementScreenBounds(transform: transform_, child: child);
    child.Arrange(finalRect: bounds_);

    var viewportBounds_ = Viewport2D.GetContentBounds(obj: this);
    if(!viewportBounds_.IsEmpty) {
      overallViewportBounds = viewportBounds_;
    }

    UniteWithBounds(transform: transform_, bounds: bounds_);

    if(!InBatchAdd) {
      Viewport2D.SetContentBounds(obj: this, value: overallViewportBounds);
      ContentBoundsChanged.Raise(sender: this);
    }
  }

  private void UniteWithBounds(CoordinateTransform transform, Rect bounds) {
    var childViewportBounds_ = bounds.ScreenToViewport(transform: transform);
    if(boundsUnionMode == BoundsUnionMode.Bounds) {
      overallViewportBounds.Union(rect: childViewportBounds_);
    }
    else {
      overallViewportBounds.Union(point: childViewportBounds_.GetCenter());
    }
  }

  private int invalidatePositionCalls;
  internal override void BeginBatchAdd() {
    base.BeginBatchAdd();
    invalidatePositionCalls = 0;
  }

  internal override void EndBatchAdd() {
    base.EndBatchAdd();
    if(plotter == null) {
      return;
    }

    UpdateContentBounds(recalculate: Children.Count > 0 && invalidatePositionCalls == 0);
  }

  public void UpdateContentBounds() {
    UpdateContentBounds(recalculate: true);
  }

  private void UpdateContentBounds(bool recalculate) {
    if(recalculate) {
      var transform_ = plotter.Viewport.Transform.WithScreenOffset(x: -translateTransform.X, y: -translateTransform.Y);
      overallViewportBounds = DataRect.Empty;
      foreach(FrameworkElement child_ in Children) {
        if(child_ != null) {
          if(child_.Visibility != Visibility.Visible) {
            continue;
          }

          var bounds_ = GetElementScreenBounds(transform: transform_, child: child_);
          UniteWithBounds(transform: transform_, bounds: bounds_);
        }
      }
    }

    Viewport2D.SetContentBounds(obj: this, value: overallViewportBounds);
    ContentBoundsChanged.Raise(sender: this);
  }

  protected override Size ArrangeOverride(Size finalSize) {
    if(plotter == null) {
      return finalSize;
    }

    var transform_ = plotter.Viewport.Transform.WithScreenOffset(x: -translateTransform.X, y: -translateTransform.Y);

    overallViewportBounds = DataRect.Empty;
    foreach(UIElement child_ in InternalChildren) {
      if(child_ != null) {
        if(child_.Visibility != Visibility.Visible) {
          continue;
        }

        var bounds_ = GetElementScreenBounds(transform: transform_, child: child_);
        child_.Arrange(finalRect: bounds_);
        UniteWithBounds(transform: transform_, bounds: bounds_);
      }
    }

    if(!InBatchAdd) {
      Viewport2D.SetContentBounds(obj: this, value: overallViewportBounds);
      ContentBoundsChanged.Raise(sender: this);
    }

    return finalSize;
  }

  public event EventHandler ContentBoundsChanged;

  protected override Size MeasureOverride(Size availableSize) {
    AvailableSize = availableSize;

    if(plotter == null) {
      if(availableSize.Width.IsInfinite() || availableSize.Height.IsInfinite()) {
        return new Size();
      }
      return availableSize;
    }

    var transform_ = plotter.Viewport.Transform;

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
}

public enum BoundsUnionMode {
  Center,
  Bounds
}