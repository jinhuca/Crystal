using System.Windows.Controls;
using System.Windows.Input;
using Crystal.Controls.PerformanceGraphs.Controls;
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

  private void OnGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    // Each ItemsControl item produces its own PerformanceGraphView; wire that instance to its
    // adapter view model so the VM pushes samples into the right column's ring buffer.
    if (sender is not PerformanceGraphView view) return;
    if (view.Graph is not { } graph) return;
    if (view.DataContext is not GpuAdapterViewModel adapter) return;
    graph.ApplyTheme(GraphThemes.Rose());
    adapter.AttachGraph(graph);
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IGpuViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
