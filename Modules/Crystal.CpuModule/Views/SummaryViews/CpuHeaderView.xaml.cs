using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>Header row for the CPU summary tile: the CPU brand and static specs (cores, cache, base
/// clock, socket) plus the throttle badge. Binds to the root ICpuViewModel inherited from the host
/// tile — the specs row from SpecsViewModel, the throttle badge from SensorsViewModel.</summary>
public partial class CpuHeaderView : UserControl {
  public CpuHeaderView() => InitializeComponent();
}
