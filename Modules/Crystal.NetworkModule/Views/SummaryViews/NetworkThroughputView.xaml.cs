using Crystal.Controls.PerformanceGraphs;
using Crystal.NetworkModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.NetworkModule.Views.SummaryViews;

/// <summary>Live throughput tile for the network summary: total download over upload, each with a
/// sparkline. Binds to the root INetworkViewModel inherited from the host tile and self-registers
/// both graphs (keyed by their GraphIdentity.Id) so the view model feeds them on each poll.</summary>
public partial class NetworkThroughputView : UserControl {
  public NetworkThroughputView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is not INetworkViewModel vm) return;
    Register(vm, NetworkDownloadGraph);
    Register(vm, NetworkUploadGraph);
  }

  private static void Register(INetworkViewModel vm, AdaptiveGraph graph) {
    if (GraphIdentity.GetId(graph) is { } id) vm.AttachGraph(id, graph);
  }
}
