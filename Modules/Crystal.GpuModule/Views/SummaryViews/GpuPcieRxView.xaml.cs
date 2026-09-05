using Crystal.Controls.PerformanceGraphs;
using Crystal.GpuModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// PCIe Rx metric tile for one GPU adapter: the live receive throughput over its history graph
/// scaled by the adapter's auto-ranging Rx maximum. Binds to the GpuAdapterViewModel inherited from
/// the per-adapter block and self-registers its graph so the view model feeds it on each poll.
/// </summary>
public partial class GpuPcieRxView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuPcieRxView"/> class.
  /// </summary>
  public GpuPcieRxView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is GpuAdapterViewModel adapter && GraphIdentity.GetId(Graph) is { } id) {
      adapter.AttachGraph(id, Graph);
    }
  }
}
