using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;
using Crystal.CpuModule.ViewModels.Interfaces;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Crystal.CpuModule.Views;

/// <summary>Compact CPU tile on the dashboard: inline specs plus segmented-bar readouts for
/// clock, voltage, utilization, temperature and power, a per-core load list, and system process
/// totals. Double-clicking opens the full CPU detail view.</summary>
public partial class CpuSummaryView : UserControl {
  public CpuSummaryView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, System.Windows.RoutedEventArgs e) {
    // Distinct accents per metric, matching the detail view. Line themes carry the vertical
    // glow-gradient fill that reads correctly under FilledLineRenderer.
    ClockBar.ApplyTheme(GraphThemes.Amber(GraphKind.Line));
    VoltageBar.ApplyTheme(GraphThemes.Emerald(GraphKind.Line));
    PowerBar.ApplyTheme(GraphThemes.Emerald(GraphKind.Line));
    UtilizationBar.ApplyTheme(GraphThemes.Rose(GraphKind.Line));
    TemperatureBar.ApplyTheme(GraphThemes.Sky(GraphKind.Line));
    FanBar.ApplyTheme(GraphThemes.FromAccent(Color.FromRgb(0x9B, 0x5A, 0xE8), GraphKind.Line));

    if (DataContext is ICpuViewModel vm)
      vm.SensorsViewModel.AttachGraphs(
          utilization: UtilizationBar, voltage: VoltageBar, clock: ClockBar,
          power: PowerBar, temperature: TemperatureBar, fan: FanBar);
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    // Open the detail view on double-click, matching the dashboard's "select or double-click"
    // affordance; a single click is ignored so the tile can host interactive content later.
    if (e.ClickCount >= 2 && DataContext is ICpuViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
