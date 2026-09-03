using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// Power meter tile for one GPU adapter: the live package-power readout over a value-banded
/// segmented range bar. Binds to the GpuAdapterViewModel inherited from the per-adapter block.
/// </summary>
public partial class GpuPowerView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuPowerView"/> class.
  /// </summary>
  public GpuPowerView() => InitializeComponent();
}
