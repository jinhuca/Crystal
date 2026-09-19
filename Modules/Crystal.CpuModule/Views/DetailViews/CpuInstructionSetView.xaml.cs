using System.Windows.Controls;

namespace Crystal.CpuModule.Views.DetailViews;

/// <summary>
/// The instruction-set feature grid for the CPU detail view: each ISA extension lit when the part
/// supports it, dimmed when not.
/// </summary>
public partial class CpuInstructionSetView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="CpuInstructionSetView"/> class.
  /// </summary>
  public CpuInstructionSetView() => InitializeComponent();
}
