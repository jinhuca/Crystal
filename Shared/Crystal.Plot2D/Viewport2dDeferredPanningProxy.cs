using Crystal.Plot2D.Common;
using Crystal.Plot2D.Transforms;

namespace Crystal.Plot2D;

internal sealed class Viewport2dDeferredPanningProxy : Viewport2D {
  private readonly Viewport2D viewport;

  internal Viewport2dDeferredPanningProxy(Viewport2D viewport) : base(host: viewport.HostElement, plotter: viewport.PlotterBase) {
    viewport.PropertyChanged += viewport_PropertyChanged;
    viewport.PanningStateChanged += viewport_PanningStateChanged;
    this.viewport = viewport;
  }

  private DataRect prevVisible;

  private void viewport_PanningStateChanged(object sender, ValueChangedEventArgs<Viewport2DPanningState> e) {
    if(e.CurrentValue == Viewport2DPanningState.Panning && e.PreviousValue == Viewport2DPanningState.NotPanning) {
      prevVisible = Visible;
    }
    else if(e.CurrentValue == Viewport2DPanningState.NotPanning && e.PreviousValue == Viewport2DPanningState.Panning) {
      notMineEvent = true;
      try {
        Visible = viewport.Visible;
      }
      finally { notMineEvent = false; }
    }
  }

  protected override DataRect CoerceVisible(DataRect newVisible) {
    if(viewport == null) {
      return new DataRect(xMin: 0, yMin: 0, width: 1, height: 1);
    }

    if(viewport.PanningState == Viewport2DPanningState.Panning) {
      return prevVisible;
    }
    return base.CoerceVisible(newVisible: newVisible);
  }

  private bool notMineEvent;
  private void viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e) {
    notMineEvent = true;
    try {
      RaisePropertyChanged(args: e);
    }
    finally {
      notMineEvent = false;
    }
  }

  public override CoordinateTransform Transform {
    get => viewport.PanningState == Viewport2DPanningState.Panning
        ? viewport.Transform.WithRects(visibleRect: prevVisible, screenRect: viewport.Output)
        : viewport.Transform;
    set => base.Transform = value;
  }

  protected override void RaisePropertyChanged(ExtendedPropertyChangedEventArgs args) {
    if(args.PropertyName == "Visible") {
      if(notMineEvent) {
        if(viewport.PanningState == Viewport2DPanningState.NotPanning) {
          base.RaisePropertyChanged(args: args);
        }
        // do nothing
      }
    }
    else {
      base.RaisePropertyChanged(args: args);
    }
  }
}