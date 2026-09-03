using Crystal.Controls.PerformanceGraphs;
using Crystal.GpuModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// PCIe throughput tile for one GPU adapter (dedicated cards only): the live Rx and Tx readouts
/// each over a sparkline. Binds to the GpuAdapterViewModel inherited from the per-adapter block and
/// self-registers both graphs so the view model feeds them on each poll.
/// </summary>
public partial class GpuPcieView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuPcieView"/> class.
  /// </summary>
  public GpuPcieView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  /// <summary>
  /// Attaches the Rx and Tx graphs to the adapter view model when the control loads, so each is fed
  /// by its <see cref="GraphIdentity"/> id on every poll.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The event data.</param>
  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is not GpuAdapterViewModel adapter) return;
    if (GraphIdentity.GetId(PcieRxGraph) is { } rxId) adapter.AttachGraph(rxId, PcieRxGraph);
    if (GraphIdentity.GetId(PcieTxGraph) is { } txId) adapter.AttachGraph(txId, PcieTxGraph);
  }
}
