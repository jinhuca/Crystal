using System;
using System.Linq;
using System.Windows.Threading;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class PlotterChildrenCollectionExtensions {
  public static void RemoveAll<T>(this PlotterChildrenCollection children) {
    var childrenToDelete = children.OfType<T>().ToList();

    foreach(var child in childrenToDelete) {
      children.Remove(item: child as IPlotterElement);
    }
  }

  public static void BeginAdd(this PlotterChildrenCollection children, IPlotterElement child) {
    children.Plotter.Dispatcher.BeginInvoke(method: (Action)(() => { children.Add(item: child); }), priority: DispatcherPriority.Send);
  }

  public static void BeginRemove(this PlotterChildrenCollection children, IPlotterElement child) {
    children.Plotter.Dispatcher.BeginInvoke(method: (Action)(() => { children.Remove(item: child); }), priority: DispatcherPriority.Send);
  }
}
