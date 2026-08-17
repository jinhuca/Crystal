using Crystal.Controls.PerformanceGraphs;
using Crystal.GpuModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>Temperature metric tile for one GPU adapter: the temperature history graph plus the live
/// temperature readout. Binds to the GpuAdapterViewModel inherited from the per-adapter item and
/// self-registers its graph so the view model feeds it on each poll.</summary>
public partial class GpuTemperatureView : UserControl {
  public GpuTemperatureView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is GpuAdapterViewModel adapter && GraphIdentity.GetId(TemperatureGraph) is { } id)
      adapter.AttachGraph(id, TemperatureGraph);
  }
}
