using System.Windows;

namespace Crystal.Plot2D.Common;

public static class PlotterEvents {
  internal static void Notify(FrameworkElement target, PlotterChangedEventArgs args) {
    PlotterAttachedEvent.Notify(target: target, args: args);
    PlotterChangedEvent.Notify(target: target, args: args);
    PlotterDetachingEvent.Notify(target: target, args: args);
  }

  public static PlotterEventHelper PlotterAttachedEvent => new(@event: PlotterBase.PlotterAttachedEvent);

  public static PlotterEventHelper PlotterDetachingEvent => new(@event: PlotterBase.PlotterDetachingEvent);

  public static PlotterEventHelper PlotterChangedEvent => new(@event: PlotterBase.PlotterChangedEvent);
}
