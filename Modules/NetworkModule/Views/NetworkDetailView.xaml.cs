using System.Windows.Controls;
using Crystal.Controls.PerformanceGraphs.Controls;
using Crystal.Controls.PerformanceGraphs.Themes;
using NetworkModule.ViewModels;

namespace NetworkModule.Views;

/// <summary>Full-scale Network view: one panel per connected interface with throughput and a live
/// utilization gauge + history graph. Reached by selecting the Network summary tile; the Back
/// control returns to the dashboard.</summary>
public partial class NetworkDetailView : UserControl {
  public NetworkDetailView() {
    InitializeComponent();
  }

  private void OnGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraphView view) return;
    if (view.Graph is not { } graph) return;
    if (view.DataContext is not NetworkAdapterViewModel adapter) return;
    graph.ApplyTheme(GraphThemes.Sky());
    adapter.AttachGraph(graph);
  }
}
