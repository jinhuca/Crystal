using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// Temperature metric tile for the CPU summary: the live package temperature and thermal headroom
/// (TjMax). Binds to the CPU SensorsViewModel inherited from the host tile.
/// </summary>
public partial class CpuTemperatureView : UserControl {
  public CpuTemperatureView() {
    InitializeComponent();
  }
}
