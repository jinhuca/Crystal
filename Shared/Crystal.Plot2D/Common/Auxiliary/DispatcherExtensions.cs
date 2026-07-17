using System;
using System.Windows.Threading;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class DispatcherExtensions {
  public static DispatcherOperation BeginInvoke(this Dispatcher dispatcher, Action action) {
    return dispatcher.BeginInvoke(method: (Delegate)action);
  }

  public static DispatcherOperation BeginInvoke(this Dispatcher dispatcher, Action action, DispatcherPriority priority) {
    return dispatcher.BeginInvoke(method: action, priority: priority);
  }

  public static void Invoke(this Dispatcher dispatcher, Action action, DispatcherPriority priority) {
    dispatcher.Invoke(callback: action, priority: priority);
  }
}
