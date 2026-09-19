using Crystal.Controls.PerformanceGraphs;
using Crystal.NetworkModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.NetworkModule.Views.SummaryViews;

/// <summary>Live throughput tile for the network summary: total download beside upload, each with a
/// history sparkline. Binds to the root INetworkViewModel inherited from the host tile and
/// self-registers both graphs so the view model feeds them on each update.</summary>
public partial class NetworkThroughputView : UserControl {
  public NetworkThroughputView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is not INetworkViewModel vm) return;
    if (GraphIdentity.GetId(DownloadGraph) is { } downloadId) vm.AttachGraph(downloadId, DownloadGraph);
    if (GraphIdentity.GetId(UploadGraph) is { } uploadId) vm.AttachGraph(uploadId, UploadGraph);
  }
}
