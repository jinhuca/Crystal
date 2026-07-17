using System.Linq;
using System.Windows.Controls;

namespace Crystal.Plot2D.Common.Auxiliary;

internal static class MenuItemExtensions {
  public static MenuItem FindChildByHeader(this MenuItem parent, string header) {
    return parent.Items.OfType<MenuItem>().FirstOrDefault(predicate: subMenu => subMenu.Header.ToString() == header);
  }
}
