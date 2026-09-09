using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// Clock metric tile for one GPU adapter: the live core-clock readout over its history graph.
/// Binds to the GpuAdapterViewModel inherited from the per-adapter block and self-registers its
/// graph so the view model feeds it on each poll.
/// </summary>
public partial class GpuClockView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuClockView"/> class.
  /// </summary>
  public GpuClockView() {
    InitializeComponent();
  }
}
