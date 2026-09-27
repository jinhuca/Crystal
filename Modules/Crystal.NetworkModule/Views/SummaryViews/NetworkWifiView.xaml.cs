using System.Windows.Controls;

namespace Crystal.NetworkModule.Views.SummaryViews;

/// <summary>Connection detail block for the network summary: a TaskManager-style list for the primary
/// interface (adapter name, SSID, DNS, connection type, IPv4/IPv6, signal), or a muted radio-state row
/// when nothing is connected. Binds to the root INetworkViewModel inherited from the host tile.</summary>
public partial class NetworkWifiView : UserControl {
  public NetworkWifiView() => InitializeComponent();
}
