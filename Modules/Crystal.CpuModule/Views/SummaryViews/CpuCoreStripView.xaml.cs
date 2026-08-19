using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// Per-core strip for the CPU summary: one row per physical core showing stacked load,
/// clock and temperature bars so the boost/thermal spread reads at a glance. Binds to the CPU
/// SensorsViewModel (its CoreLoads collection) inherited from the host tile.
/// </summary>
public partial class CpuCoreStripView : UserControl {
  public CpuCoreStripView() => InitializeComponent();
}
