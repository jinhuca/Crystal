using System.Windows;
using System.Windows.Controls;
using CpuModule.ViewModels.Interfaces;
using Crystal.Controls.MeterGauges.Themes;
using Crystal.Controls.PerformanceGraphs.Themes;

namespace CpuModule.Views;

/// <summary>Full-scale CPU view: static specs, five live gauges + history graphs, and
/// the instruction-set grid. Reached by selecting the CPU summary tile on the dashboard.</summary>
public partial class CpuDetailView : UserControl {
  public CpuDetailView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    // The wrapped PerformanceGraph instances are produced by each view's control template,
    // so they aren't available until after the template is applied (i.e. at Loaded, not ctor).
    if (UtilizationView.Graph is not { } utilization) return;
    if (VoltageView.Graph is not { } voltage) return;
    if (ClockView.Graph is not { } clock) return;
    if (PowerView.Graph is not { } power) return;
    if (TemperatureView.Graph is not { } temperature) return;

    // Accent each plot to match the reference image, and mirror the accent onto its gauge.
    utilization.ApplyTheme(GraphThemes.Rose());
    voltage.ApplyTheme(GraphThemes.Emerald());
    clock.ApplyTheme(GraphThemes.Amber());
    power.ApplyTheme(GraphThemes.Rose());
    temperature.ApplyTheme(GraphThemes.Rose());

    if (DataContext is ICpuViewModel vm)
      vm.SensorsViewModel.AttachGraphs(utilization, voltage, clock, power, temperature);
  }
}
