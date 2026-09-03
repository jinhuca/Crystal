using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// Hot-spot temperature meter tile for one GPU adapter (dedicated cards only): the live hot-spot
/// readout over a value-banded segmented range bar. Binds to the GpuAdapterViewModel inherited from
/// the per-adapter block.
/// </summary>
public partial class GpuHotSpotView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuHotSpotView"/> class.
  /// </summary>
  public GpuHotSpotView() => InitializeComponent();
}
