using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;

namespace Crystal.Plot2D.Charts;

public sealed class RemoveAll : IPlotterElement {
  private Type type;

  [NotNull]
  public Type Type {
    get => type;
    set {
      ArgumentNullException.ThrowIfNull(value);
      type = value;
    }
  }

  private PlotterBase plotter;
  public PlotterBase Plotter => plotter;

  public void OnPlotterAttached(PlotterBase plotter) {
    this.plotter = plotter;
    if(type != null) {
      plotter.Children.RemoveAll(type: type);
    }
  }

  public void OnPlotterDetaching(PlotterBase plotter) {
    this.plotter = null;
  }
}