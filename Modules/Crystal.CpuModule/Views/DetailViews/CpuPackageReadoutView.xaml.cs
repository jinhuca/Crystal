using System.Windows.Controls;

namespace Crystal.CpuModule.Views.DetailViews;

/// <summary>
/// Package-level live readout strip for the CPU detail view: MSR aggregates, AMD SoC/current,
/// package C-states, and CPU fan.
/// </summary>
public partial class CpuPackageReadoutView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="CpuPackageReadoutView"/> class.
  /// </summary>
  public CpuPackageReadoutView() => InitializeComponent();
}
