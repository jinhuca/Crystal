using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// Power metric tile for the CPU summary: the live package power and its limits/currents
/// (PL1/PL2 on Intel, TDC/EDC on AMD). Binds to the CPU SensorsViewModel inherited from the host tile.
/// </summary>
public partial class CpuPowerView : UserControl {
  public CpuPowerView() {
    InitializeComponent();
  }
}
