using System.Windows.Controls;

namespace Crystal.NetworkModule.Views.SummaryViews;

/// <summary>Top-talkers tile for the network summary: the busiest processes on the network right
/// now (rate descending, capped), or a muted note when the ETW session isn't running. Binds to the
/// root INetworkViewModel inherited from the host tile.</summary>
public partial class NetworkTopTalkersView : UserControl {
  public NetworkTopTalkersView() => InitializeComponent();
}
