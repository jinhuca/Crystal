using System.Windows;
using System.Windows.Threading;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class DependencyObjectExtensions {
  public static T GetValueSync<T>(this DependencyObject d, DependencyProperty property) {
    object value = null;
    d.Dispatcher.Invoke(callback: () => { value = d.GetValue(dp: property); }, priority: DispatcherPriority.Send);
    return (T)value;
  }
}
