using Crystal.NetworkModule.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.NetworkModule.Views;

/// <summary>Compact Network tile on the dashboard: composes the header, live throughput, top-talkers
/// and Wi-Fi tiles (defined in Views/SummaryViews). Double-clicking opens the full detail view.</summary>
public partial class NetworkSummaryView : UserControl {
  public NetworkSummaryView() => InitializeComponent();

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is INetworkViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
