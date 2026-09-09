using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// Voltage metric tile for the CPU summary: the live core voltage and SoC voltage. Binds to the
/// CPU SensorsViewModel inherited from the host tile.
/// </summary>
public partial class CpuVoltageView : UserControl {
  public CpuVoltageView() {
    InitializeComponent();
  }
}
