using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// Clock metric tile for the CPU summary: the live package clock, effective clock and bus speed
/// over a value-banded segmented range bar. Binds to the CPU SensorsViewModel inherited from the
/// host tile. The clock history graph now lives in the CPU detail view.
/// </summary>
public partial class CpuClockView : UserControl {
  public CpuClockView() => InitializeComponent();
}
