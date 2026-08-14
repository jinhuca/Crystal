using System.Windows;
using System.Windows.Controls;
using Crystal.Controls.PerformanceGraphs.Controls;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;
using Crystal.NetworkModule.ViewModels;

namespace Crystal.NetworkModule.Views;

/// <summary>Full-scale Network view: one panel per connected interface with the current
/// download/upload readout and a live history graph for each, plus a per-process top-talkers table.
/// Reached by selecting the Network summary tile; the Back control returns to the dashboard.</summary>
public partial class NetworkDetailView : UserControl {
  public NetworkDetailView() {
    InitializeComponent();
  }

  // Clicking a top-talkers column header sorts by that column (toggling asc/desc); the sort key
  // lives on the column via GridViewSort.SortProperty.
  private void OnTopTalkersHeaderClick(object sender, RoutedEventArgs e) {
    if (e.OriginalSource is not GridViewColumnHeader header) return;
    if (header.Column is null) return;
    var sortProperty = GridViewSort.GetSortProperty(header.Column);
    if (string.IsNullOrEmpty(sortProperty)) return;
    if (DataContext is INetworkViewModel vm) vm.SortTopTalkersBy(sortProperty);
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

  private void OnSignalGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraphView view) return;
    if (view.Graph is not { } graph) return;
    if (view.DataContext is not NetworkAdapterViewModel adapter) return;
    graph.ApplyTheme(GraphThemes.Sky(GraphKind.Line));
    adapter.AttachSignalGraph(graph);
  }
}
