using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// 3D-engine utilization meter tile for one GPU adapter: the live 3D load readout over a
/// value-banded segmented range bar. Binds to the GpuAdapterViewModel inherited from the
/// per-adapter block.
/// </summary>
public partial class Gpu3DView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="Gpu3DView"/> class.
  /// </summary>
  public Gpu3DView() => InitializeComponent();
}
