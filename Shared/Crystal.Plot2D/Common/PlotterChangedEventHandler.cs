using System;
using System.Windows;

namespace Crystal.Plot2D.Common;

public sealed class PlotterChangedEventArgs : RoutedEventArgs {
  internal PlotterChangedEventArgs(PlotterBase prevPlotter, PlotterBase currPlotter, RoutedEvent routedEvent) : base(routedEvent: routedEvent) {
    if(prevPlotter == null && currPlotter == null) {
      throw new ArgumentException(message: "Both Plotters cannot be null.");
    }

    PreviousPlotter = prevPlotter;
    CurrentPlotter = currPlotter;
  }

  internal PlotterBase PreviousPlotter { get; }
  internal PlotterBase CurrentPlotter { get; }
}
