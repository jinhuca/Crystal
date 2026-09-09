using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// Clock metric tile for the CPU summary: the live package clock, effective clock and bus speed.
/// Binds to the CPU SensorsViewModel inherited from the host tile.
/// </summary>
public partial class CpuClockView : UserControl {
  public CpuClockView() {
    InitializeComponent();
  }
}
