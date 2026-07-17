using System.Windows;
using System.Windows.Data;

namespace Crystal.Plot2D.Common.Auxiliary.MarkupExtensions;

public class SelfBinding : Binding {
  public SelfBinding() {
    RelativeSource = new RelativeSource { Mode = RelativeSourceMode.Self };
  }

  public SelfBinding(string propertyPath) : this() {
    Path = new PropertyPath(path: propertyPath);
  }
}
