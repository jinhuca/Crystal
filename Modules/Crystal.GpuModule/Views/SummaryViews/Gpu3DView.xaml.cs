using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// 3D-engine utilization metric tile for one GPU adapter: the live 3D load readout over its
/// value-banded history graph. Binds to the GpuAdapterViewModel inherited from the per-adapter
/// block and self-registers its graph so the view model feeds it on each poll.
/// </summary>
public partial class Gpu3DView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="Gpu3DView"/> class.
  /// </summary>
  public Gpu3DView() {
    InitializeComponent();
  }
}
