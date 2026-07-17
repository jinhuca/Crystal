using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Common;

public sealed class Footer : ContentControl, IPlotterElement {
  public Footer() {
    FontSize = 12;
    HorizontalAlignment = HorizontalAlignment.Center;
    Margin = new Thickness(left: 0, top: 3, right: 0, bottom: 3);
  }

  public PlotterBase Plotter { get; private set; }

  void IPlotterElement.OnPlotterAttached(PlotterBase plotter) {
    Plotter = plotter;
    plotter.FooterPanel.Children.Add(element: this);
  }

  void IPlotterElement.OnPlotterDetaching(PlotterBase plotter) {
    Plotter = null;
    plotter.FooterPanel.Children.Remove(element: this);
  }
}
