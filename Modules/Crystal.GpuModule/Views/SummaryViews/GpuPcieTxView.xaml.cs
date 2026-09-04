using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// PCIe Tx meter tile for one GPU adapter: the live transmit throughput over a segmented range bar
/// scaled by the adapter's auto-ranging Tx maximum. Binds to the GpuAdapterViewModel inherited from
/// the per-adapter block.
/// </summary>
public partial class GpuPcieTxView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuPcieTxView"/> class.
  /// </summary>
  public GpuPcieTxView() => InitializeComponent();
}
