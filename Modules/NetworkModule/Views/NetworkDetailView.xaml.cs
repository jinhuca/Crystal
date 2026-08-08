using System.Windows.Controls;
using Crystal.Controls.PerformanceGraphs.Controls;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;
using NetworkModule.ViewModels;

namespace NetworkModule.Views;

/// <summary>Full-scale Network view: one panel per connected interface with the current
/// download/upload readout and a live history graph for each. Reached by selecting the Network
/// summary tile; the Back control returns to the dashboard.</summary>
public partial class NetworkDetailView : UserControl {
  public NetworkDetailView() {
    InitializeComponent();
  }

  private void OnDownloadGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraphView view) return;
    if (view.Graph is not { } graph) return;
    if (view.DataContext is not NetworkAdapterViewModel adapter) return;
    graph.ApplyTheme(GraphThemes.Emerald(GraphKind.SegmentedBar));
    adapter.AttachDownloadGraph(graph);
  }

  private void OnUploadGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraphView view) return;
    if (view.Graph is not { } graph) return;
    if (view.DataContext is not NetworkAdapterViewModel adapter) return;
    graph.ApplyTheme(GraphThemes.Amber(GraphKind.SegmentedBar));
    adapter.AttachUploadGraph(graph);
  }
}
