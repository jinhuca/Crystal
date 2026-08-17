using System.Windows.Controls;

namespace Crystal.NetworkModule.Views.SummaryViews;

/// <summary>Header row for the network summary tile: accent tick, title, and the active-interface
/// count. Binds to the root INetworkViewModel inherited from the host tile.</summary>
public partial class NetworkHeaderView : UserControl {
  public NetworkHeaderView() => InitializeComponent();
}
