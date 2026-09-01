using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// Power metric tile for the CPU summary: the live package power and its limits/currents
/// (PL1/PL2 on Intel, TDC/EDC on AMD) over a value-banded segmented range bar. Binds to the CPU
/// SensorsViewModel inherited from the host tile. The power history graph now lives in the CPU
/// detail view.
/// </summary>
public partial class CpuPowerView : UserControl {
  public CpuPowerView() => InitializeComponent();
}
