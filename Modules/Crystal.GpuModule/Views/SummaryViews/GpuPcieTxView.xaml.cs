using System.Windows;
using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// PCIe Tx throughput metric tile for one GPU adapter: the live outbound-bandwidth readout over its
/// history graph. Binds to the GpuAdapterViewModel inherited from the per-adapter block and
/// self-registers its graph so the view model feeds it on each poll.
/// </summary>
public partial class GpuPcieTxView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuPcieTxView"/> class.
  /// </summary>
  public GpuPcieTxView() {
    InitializeComponent();
  }

  private void OnGraphLoaded(object sender, RoutedEventArgs e) => GpuGraphAttach.Attach(sender);
}
