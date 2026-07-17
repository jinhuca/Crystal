using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.Common.Auxiliary;

internal static class VisualTreeHelperHelper {
  public static DependencyObject GetParent(DependencyObject target, int depth) {
    var parent = target;
    do {
      parent = VisualTreeHelper.GetParent(reference: parent);
      if(parent == null) {
        break;
      }
    } while(--depth > 0);

    return parent;
  }
}
