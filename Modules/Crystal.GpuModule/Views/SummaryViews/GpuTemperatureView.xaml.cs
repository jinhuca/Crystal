using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// Temperature metric tile for one GPU adapter: the live core-temperature readout over its
/// value-banded history graph. Binds to the GpuAdapterViewModel inherited from the per-adapter
/// block and self-registers its graph so the view model feeds it on each poll.
/// </summary>
public partial class GpuTemperatureView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuTemperatureView"/> class.
  /// </summary>
  public GpuTemperatureView() {
    InitializeComponent();
  }
}
