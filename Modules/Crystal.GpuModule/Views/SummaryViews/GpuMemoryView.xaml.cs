using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// Memory-used meter tile for one GPU adapter: the "used / total GB (percent)" readout over a
/// segmented range bar driven by the used-percent. Binds to the GpuAdapterViewModel inherited from
/// the per-adapter block.
/// </summary>
public partial class GpuMemoryView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuMemoryView"/> class.
  /// </summary>
  public GpuMemoryView() => InitializeComponent();
}
