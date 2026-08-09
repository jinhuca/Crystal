using System.Windows.Controls;
using System.Windows.Input;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;
using MemoryModule.ViewModels;

namespace MemoryModule.Views;

/// <summary>Compact Memory tile on the dashboard: total capacity, slot count, top speed, plus
/// live utilization-% and used-GB history graphs each with a big-value readout (matching the GPU
/// tile). Double-clicking opens the full detail view.</summary>
public partial class MemorySummaryView : UserControl {
  public MemorySummaryView() {
    InitializeComponent();
  }

  private void OnLoadGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    graph.ApplyTheme(GraphThemes.Rose(GraphKind.Line));
    if (DataContext is IMemoryViewModel vm)
      vm.AttachGraph(graph);
  }

  private void OnUsedGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    // Sky matches the detail view's "Memory usage" graph so the shared used-GB metric reads the
    // same accent across both views.
    graph.ApplyTheme(GraphThemes.Sky(GraphKind.Line));
    if (DataContext is IMemoryViewModel vm)
      vm.AttachUsedGraph(graph);
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IMemoryViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
