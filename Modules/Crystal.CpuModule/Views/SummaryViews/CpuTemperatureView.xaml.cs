using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// Temperature metric tile for the CPU summary: the live package temperature and thermal headroom
/// (TjMax) over a value-banded segmented range bar. Binds to the CPU SensorsViewModel inherited
/// from the host tile. The temperature history graph now lives in the CPU detail view.
/// </summary>
public partial class CpuTemperatureView : UserControl {
  public CpuTemperatureView() => InitializeComponent();
}
