using Crystal.Controls.PerformanceGraphs;
using Crystal.CpuModule.ViewModels.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.CpuModule.Views.DetailViews;

/// <summary>
/// The CPU metric history graphs (clock, voltage, temperature, power, fan) relocated from the
/// summary tiles. Each graph self-registers with the inherited SensorsViewModel so it is fed on
/// every poll.
/// </summary>
public partial class CpuHistoryGraphsView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="CpuHistoryGraphsView"/> class.
  /// </summary>
  public CpuHistoryGraphsView() => InitializeComponent();

  /// <summary>
  /// Self-registers a metric history graph with the inherited SensorsViewModel so it is fed on
  /// every poll. The graph host inherits the SensorsViewModel as its DataContext.
  /// </summary>
  private void OnGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is ISingleSeriesGraph graph
        && sender is FrameworkElement { DataContext: ICpuSensorViewModel vm } fe
        && GraphIdentity.GetId(fe) is { } id) {
      vm.AttachGraph(id, graph);
    }
  }
}
