using Crystal.Plot2D.Common;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Charts;

public abstract class ContentGraph : ContentControl, IPlotterElement {
  static ContentGraph() {
    EventManager.RegisterClassHandler(classType: typeof(ContentGraph), routedEvent: PlotterBase.PlotterChangedEvent, handler: new PlotterChangedEventHandler(OnPlotterChanged));
  }

  private static void OnPlotterChanged(object sender, PlotterChangedEventArgs e) {
    var owner_ = (ContentGraph)sender;
    owner_.OnPlotterChanged(e: e);
  }

  private void OnPlotterChanged(PlotterChangedEventArgs e) {
    if(plotter == null && e.CurrentPlotter != null) {
      plotter = (PlotterBase)e.CurrentPlotter;
      plotter.Viewport.PropertyChanged += Viewport_PropertyChanged;
      OnPlotterAttached();
    }

    if(plotter == null || e.PreviousPlotter == null) {
      return;
    }

    OnPlotterDetaching();
    plotter.Viewport.PropertyChanged -= Viewport_PropertyChanged;
    plotter = null;
  }

  #region IPlotterElement Members

  private void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e) {
    OnViewportPropertyChanged(e: e);
  }

  protected virtual void OnViewportPropertyChanged(ExtendedPropertyChangedEventArgs e) { }

  protected virtual Panel HostPanel => plotter.CentralGrid;

  void IPlotterElement.OnPlotterAttached(PlotterBase plotter) {
    this.plotter = (PlotterBase)plotter;
    HostPanel.Children.Add(element: this);
    this.plotter.Viewport.PropertyChanged += Viewport_PropertyChanged;
    OnPlotterAttached();
  }

  protected virtual void OnPlotterAttached() { }

  void IPlotterElement.OnPlotterDetaching(PlotterBase plotter) {
    OnPlotterDetaching();
    this.plotter.Viewport.PropertyChanged -= Viewport_PropertyChanged;
    HostPanel.Children.Remove(element: this);
    this.plotter = null;
  }

  protected virtual void OnPlotterDetaching() { }

  private PlotterBase plotter;
  protected PlotterBase Plotter2D => plotter;

  PlotterBase IPlotterElement.Plotter => plotter;

  #endregion
}