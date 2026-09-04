using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// PCIe Rx meter tile for one GPU adapter: the live receive throughput over a segmented range bar
/// scaled by the adapter's auto-ranging Rx maximum. Binds to the GpuAdapterViewModel inherited from
/// the per-adapter block.
/// </summary>
public partial class GpuPcieRxView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuPcieRxView"/> class.
  /// </summary>
  public GpuPcieRxView() => InitializeComponent();
}
