using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// Hot-spot temperature metric tile for one GPU adapter (dedicated cards only): the live hot-spot
/// readout over its value-banded history graph. Binds to the GpuAdapterViewModel inherited from the
/// per-adapter block and self-registers its graph so the view model feeds it on each poll.
/// </summary>
public partial class GpuHotSpotView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuHotSpotView"/> class.
  /// </summary>
  public GpuHotSpotView() {
    InitializeComponent();
  }
}
