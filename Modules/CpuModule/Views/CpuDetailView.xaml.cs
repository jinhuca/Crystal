using System.Windows.Controls;

namespace CpuModule.Views;

/// <summary>Full-scale CPU view: static specs, the per-core clocks table, and the
/// instruction-set grid. Reached by selecting the CPU summary tile on the dashboard.</summary>
public partial class CpuDetailView : UserControl {
  public CpuDetailView() => InitializeComponent();
}
