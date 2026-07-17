using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace Crystal.Plot2D.Common;

[DebuggerDisplay(value: "Count = {Cache.Count}")]
public sealed class VisualBindingCollection {
  internal Dictionary<IPlotterElement, UIElement> Cache { get; } = new();
  public UIElement this[IPlotterElement element] => Cache[key: element];
  public bool Contains(IPlotterElement element) => Cache.ContainsKey(key: element);
}
