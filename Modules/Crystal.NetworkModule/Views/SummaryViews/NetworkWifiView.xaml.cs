using System.Windows.Controls;

namespace Crystal.NetworkModule.Views.SummaryViews;

/// <summary>Wi-Fi tile for the network summary: the connected radio's SSID/signal/link/security/BSSID,
/// or a muted status row when a radio is present but not connected. Binds to the root
/// INetworkViewModel inherited from the host tile.</summary>
public partial class NetworkWifiView : UserControl {
  public NetworkWifiView() => InitializeComponent();
}
