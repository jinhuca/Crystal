using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// Power metric tile for one GPU adapter: the live package-power readout over its value-banded
/// history graph. Binds to the GpuAdapterViewModel inherited from the per-adapter block and
/// self-registers its graph so the view model feeds it on each poll.
/// </summary>
public partial class GpuPowerView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuPowerView"/> class.
  /// </summary>
  public GpuPowerView() {
    InitializeComponent();
  }
}
