using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// Temperature meter tile for one GPU adapter: the live core-temperature readout over a
/// value-banded segmented range bar. Binds to the GpuAdapterViewModel inherited from the
/// per-adapter block.
/// </summary>
public partial class GpuTemperatureView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuTemperatureView"/> class.
  /// </summary>
  public GpuTemperatureView() => InitializeComponent();
}
