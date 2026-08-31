using System.Collections.ObjectModel;

namespace Crystal.Controls.Demo.Support;

/// <summary>
/// Minimal stand-in for a real view-model, existing only to demonstrate
/// <see cref="Crystal.Controls.PerformanceGraphs.PerformanceGraphLite.ValuesSource"/>: a plain
/// <see cref="ObservableCollection{T}"/> that <see cref="MainWindow"/>'s own
/// <see cref="System.Windows.Threading.DispatcherTimer"/> tick appends to directly, with no
/// <c>AddValue</c> call anywhere in this demo's code-behind for the tile bound to it - the graph's
/// data arrives purely through the collection and its <see cref="System.Collections.Specialized.INotifyCollectionChanged"/>
/// notifications, exactly as a real MVVM consumer's would.
/// </summary>
internal sealed class LiteValuesSourceDemo {
  /// <summary>
  /// The bound collection. Capped by the tick handler to the target graph's own
  /// <see cref="Crystal.Controls.PerformanceGraphs.PerformanceGraphLite.Capacity"/> via
  /// <c>RemoveAt(0)</c> once it grows past it - <see cref="ValuesSource"/> only ever appends into
  /// the graph's own ring buffer (which already evicts on its own), it never shrinks the source
  /// collection for you, so a long-running real consumer needs to do this same capping itself or
  /// this collection would otherwise grow without bound for as long as the app keeps ticking.
  /// </summary>
  public ObservableCollection<double> UtilizationSamples { get; } = new();
}
