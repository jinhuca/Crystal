using Crystal.Controls.PerformanceGraphs;
using Crystal.NetworkModule.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.NetworkModule.Views;

/// <summary>Compact Network tile on the dashboard: active-interface count plus live total
/// download and upload throughput. Double-clicking opens the full detail view.</summary>
public partial class NetworkSummaryView : UserControl {
  public NetworkSummaryView() {
    InitializeComponent();
  }

  // Appearance (kind/accent/category/history) is owned by the graph-settings feature, keyed by
  // GraphIdentity.Id in XAML; the handlers only wire each sparkline's sample buffer to the view model.
  private void OnDownloadGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    if (DataContext is INetworkViewModel vm) vm.AttachDownloadGraph(graph);
  }

  private void OnUploadGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    if (DataContext is INetworkViewModel vm) vm.AttachUploadGraph(graph);
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is INetworkViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
