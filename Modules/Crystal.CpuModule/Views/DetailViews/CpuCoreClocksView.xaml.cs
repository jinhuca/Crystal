using System.Windows.Controls;

namespace Crystal.CpuModule.Views.DetailViews;

/// <summary>
/// The per-core clock table for the CPU detail view: one row per physical core with its clock,
/// multiplier, thermal headroom and power readouts plus a stack of per-thread load bars.
/// </summary>
public partial class CpuCoreClocksView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="CpuCoreClocksView"/> class.
  /// </summary>
  public CpuCoreClocksView() => InitializeComponent();
}
