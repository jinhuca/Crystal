using System;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class PlotterElementExtensions {
  public static void RemoveFromPlotter(this IPlotterElement element) {
    ArgumentNullException.ThrowIfNull(element);
    element.Plotter?.Children.Remove(item: element);
  }

  public static void AddToPlotter(this IPlotterElement element, PlotterBase plotter) {
    ArgumentNullException.ThrowIfNull(element);
    ArgumentNullException.ThrowIfNull(plotter);
    plotter.Children.Add(item: element);
  }
}
