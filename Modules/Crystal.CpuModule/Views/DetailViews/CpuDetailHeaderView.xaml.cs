using System.Windows.Controls;

namespace Crystal.CpuModule.Views.DetailViews;

/// <summary>
/// Header of the CPU detail view: the back button and the static spec readout (identity, topology,
/// caches, speeds, virtualization).
/// </summary>
public partial class CpuDetailHeaderView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="CpuDetailHeaderView"/> class.
  /// </summary>
  public CpuDetailHeaderView() => InitializeComponent();
}
