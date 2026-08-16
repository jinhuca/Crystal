using Crystal.Controls.PerformanceGraphs;
using Crystal.GpuModule.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.GpuModule.Views;

/// <summary>Compact GPU tile on the dashboard: one column per adapter (integrated / dedicated),
/// each with a Load gauge and a load-percentage history graph. Double-clicking opens the full
/// GPU detail view.</summary>
public partial class GpuSummaryView : UserControl {
  public GpuSummaryView() {
    InitializeComponent();
  }

  // Each ItemsControl item produces its own PerformanceGraph; wire that instance to its adapter view
  // model so the VM pushes samples into the right column's ring buffer. Appearance (kind/accent/
  // category/history) is owned by the graph-settings feature, keyed by GraphIdentity.Id in XAML.
  private void OnLoadGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    if (graph.DataContext is not GpuAdapterViewModel adapter) return;
    adapter.AttachGraph(graph);
  }

  private void OnTemperatureGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    if (graph.DataContext is not GpuAdapterViewModel adapter) return;
    adapter.AttachTemperatureGraph(graph);
  }

  private void OnClockGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    if (graph.DataContext is not GpuAdapterViewModel adapter) return;
    adapter.AttachClockGraph(graph);
  }

  private void OnPowerGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    if (graph.DataContext is not GpuAdapterViewModel adapter) return;
    adapter.AttachPowerGraph(graph);
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IGpuViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
