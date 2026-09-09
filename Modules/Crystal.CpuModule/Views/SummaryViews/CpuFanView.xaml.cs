using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// CPU fan metric tile for the CPU summary: the live fan readout (RPM, or PWM percentage on
/// tachometer-less laptops). Binds to the CPU SensorsViewModel inherited from the host tile.
/// </summary>
public partial class CpuFanView : UserControl {
  public CpuFanView() {
    InitializeComponent();
  }
}
