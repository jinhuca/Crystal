using System.Windows.Controls;
using System.Windows.Input;
using Crystal.Controls.PerformanceGraphs.Themes;
using NetworkModule.ViewModels;

namespace NetworkModule.Views;

/// <summary>Compact Network tile on the dashboard: active-interface count plus live combined
/// download/upload throughput and a utilization-percentage history graph. Double-clicking opens
/// the full detail view.</summary>
public partial class NetworkSummaryView : UserControl {
  public NetworkSummaryView() {
    InitializeComponent();
  }

  private void OnGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (UtilizationView.Graph is not { } utilization) return;
    utilization.ApplyTheme(GraphThemes.Sky());
    if (DataContext is INetworkViewModel vm)
      vm.AttachGraph(utilization);
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is INetworkViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
