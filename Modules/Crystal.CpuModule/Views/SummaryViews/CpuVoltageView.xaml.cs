using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// Voltage metric tile for the CPU summary: the live core voltage and SoC voltage over a
/// value-banded segmented range bar. Binds to the CPU SensorsViewModel inherited from the host
/// tile. The voltage history graph now lives in the CPU detail view.
/// </summary>
public partial class CpuVoltageView : UserControl {
  public CpuVoltageView() => InitializeComponent();
}
