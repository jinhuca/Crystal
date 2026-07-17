using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Common;

public sealed class Header : ContentControl, IPlotterElement {
  public Header() {
    FontSize = 12;
    HorizontalAlignment = HorizontalAlignment.Center;
    Margin = new Thickness(left: 0, top: 3, right: 0, bottom: 3);
  }

  public PlotterBase Plotter { get; private set; }

  public void OnPlotterAttached(PlotterBase plotter) {
    Plotter = plotter;
    plotter.HeaderPanel.Children.Add(element: this);
  }

  public void OnPlotterDetaching(PlotterBase plotter) {
    Plotter = null;
    plotter.HeaderPanel.Children.Remove(element: this);
  }
}