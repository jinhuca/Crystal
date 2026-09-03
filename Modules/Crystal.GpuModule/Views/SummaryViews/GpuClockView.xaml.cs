using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// Clock meter tile for one GPU adapter: the live core-clock readout over a segmented range bar.
/// Binds to the GpuAdapterViewModel inherited from the per-adapter block.
/// </summary>
public partial class GpuClockView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuClockView"/> class.
  /// </summary>
  public GpuClockView() => InitializeComponent();
}
