using System.Windows;
using System.Windows.Data;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class BindingHelper {
  public static Binding CreateAttachedPropertyBinding(DependencyProperty attachedProperty) {
    return new Binding { Path = new PropertyPath(path: "(0)", pathParameters: attachedProperty) };
  }
}
