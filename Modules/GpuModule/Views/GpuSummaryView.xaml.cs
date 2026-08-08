using System.Windows.Controls;
using System.Windows.Input;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;
using GpuModule.ViewModels;

namespace GpuModule.Views;

/// <summary>Compact GPU tile on the dashboard: one column per adapter (integrated / dedicated),
/// each with a Load gauge and a load-percentage history graph. Double-clicking opens the full
/// GPU detail view.</summary>
public partial class GpuSummaryView : UserControl {
  public GpuSummaryView() {
    InitializeComponent();
  }

  private void OnLoadGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    // Each ItemsControl item produces its own PerformanceGraph; wire that instance to its
    // adapter view model so the VM pushes samples into the right column's ring buffer.
    if (sender is not PerformanceGraph graph) return;
    if (graph.DataContext is not GpuAdapterViewModel adapter) return;
    graph.ApplyTheme(GraphThemes.Rose(GraphKind.Line));
    adapter.AttachGraph(graph);
  }

  private void OnTemperatureGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    if (graph.DataContext is not GpuAdapterViewModel adapter) return;
    graph.ApplyTheme(GraphThemes.Sky(GraphKind.Line));
    adapter.AttachTemperatureGraph(graph);
  }

  private void OnClockGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    if (graph.DataContext is not GpuAdapterViewModel adapter) return;
    graph.ApplyTheme(GraphThemes.Amber(GraphKind.Line));
    adapter.AttachClockGraph(graph);
  }

  private void OnPowerGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    if (graph.DataContext is not GpuAdapterViewModel adapter) return;
    graph.ApplyTheme(GraphThemes.Emerald(GraphKind.Line));
    adapter.AttachPowerGraph(graph);
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IGpuViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
