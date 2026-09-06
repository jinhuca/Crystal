using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// Static-specs tile for one GPU adapter: a compact restatement of the display mode, driver version,
/// driver date, and refresh rate also shown in GpuDetailView. Binds to the GpuAdapterViewModel
/// inherited from the per-adapter block; it holds no live graph, so there is nothing to self-register.
/// </summary>
public partial class GpuInfoView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuInfoView"/> class.
  /// </summary>
  public GpuInfoView() {
    InitializeComponent();
  }
}
