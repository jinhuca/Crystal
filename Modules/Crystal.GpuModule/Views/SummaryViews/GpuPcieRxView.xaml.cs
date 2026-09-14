using System.Windows;
using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// PCIe Rx throughput metric tile for one GPU adapter: the live inbound-bandwidth readout over its
/// history graph. Binds to the GpuAdapterViewModel inherited from the per-adapter block and
/// self-registers its graph so the view model feeds it on each poll.
/// </summary>
public partial class GpuPcieRxView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuPcieRxView"/> class.
  /// </summary>
  public GpuPcieRxView() {
    InitializeComponent();
  }

  private void OnGraphLoaded(object sender, RoutedEventArgs e) => GpuGraphAttach.Attach(sender);
}
