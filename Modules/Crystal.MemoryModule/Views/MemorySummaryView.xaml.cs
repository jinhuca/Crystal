using Crystal.Controls.PerformanceGraphs;
using Crystal.MemoryModule.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.MemoryModule.Views;

/// <summary>Compact Memory tile on the dashboard: total capacity, slot count, top speed, plus
/// live utilization-% and used-GB history graphs each with a big-value readout (matching the GPU
/// tile). Double-clicking opens the full detail view.</summary>
public partial class MemorySummaryView : UserControl {
  public MemorySummaryView() {
    InitializeComponent();
  }

  // Appearance (kind/accent/category/history) is owned by the graph-settings feature, keyed by
  // GraphIdentity.Id in XAML; the handlers only wire each graph's sample buffer to the view model.
  private void OnLoadGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    if (DataContext is IMemoryViewModel vm)
      vm.AttachGraph(graph);
  }

  private void OnUsedGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    if (DataContext is IMemoryViewModel vm)
      vm.AttachUsedGraph(graph);
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IMemoryViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
