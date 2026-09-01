using Crystal.Controls.PerformanceGraphs;
using Crystal.CpuModule.ViewModels.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.CpuModule.Views;

/// <summary>
/// Full-scale CPU view: static specs, the metric history graphs (relocated from the summary
/// tiles), the per-core clocks table, and the instruction-set grid. Reached by selecting the CPU
/// summary tile on the dashboard.
/// </summary>
public partial class CpuDetailView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="CpuDetailView"/> class.
  /// </summary>
  public CpuDetailView() => InitializeComponent();

  /// <summary>
  /// Self-registers a relocated metric history graph with this view's own SensorsViewModel so it
  /// is fed on every poll. The graph host inherits the SensorsViewModel as its DataContext.
  /// </summary>
  private void OnGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is ISingleSeriesGraph graph
        && sender is FrameworkElement { DataContext: ICpuSensorViewModel vm } fe
        && GraphIdentity.GetId(fe) is { } id) {
      vm.AttachGraph(id, graph);
    }
  }
}
