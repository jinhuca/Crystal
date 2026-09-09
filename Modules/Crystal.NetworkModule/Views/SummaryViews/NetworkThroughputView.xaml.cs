using System.Windows.Controls;

namespace Crystal.NetworkModule.Views.SummaryViews;

/// <summary>Live throughput tile for the network summary: total download over upload. Binds to the
/// root INetworkViewModel inherited from the host tile.</summary>
public partial class NetworkThroughputView : UserControl {
  public NetworkThroughputView() {
    InitializeComponent();
  }
}
