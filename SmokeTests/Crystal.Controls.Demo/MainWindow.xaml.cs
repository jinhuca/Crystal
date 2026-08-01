using System.Windows;
using System.Windows.Threading;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;

namespace Crystal.Controls.Demo; 
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
  private readonly Random _random = new();
  private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

  private double _utilization = 20;

  public MainWindow() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    // The wrapped PerformanceGraph is created by the control template, so it isn't
    // available until the template has been applied (after Loaded, not in the ctor).
    // MinValue/MaxValue are set in XAML on the view and auto-wire to the graph's scale.
    UtilizationGraph.ApplyTheme(GraphThemes.Rose());
    VoltageGraph.ApplyTheme(GraphThemes.Emerald());
    VoltageGaugeView.Gauge?.ApplyTheme(Crystal.Controls.MeterGauges.Themes.GaugeThemes.Emerald());
    VoltageBar.Bar?.ApplyTheme(Crystal.Controls.RangeBars.Themes.RangeBarThemes.Emerald());

    _timer.Tick += OnTick;
    _timer.Start();
  }

  private PerformanceGraph UtilizationGraph =>
      UtilizationView.Graph ?? throw new InvalidOperationException("Utilization graph template not applied.");

  private PerformanceGraph VoltageGraph =>
      VoltageView.Graph ?? throw new InvalidOperationException("Voltage graph template not applied.");

  private void OnTick(object? sender, EventArgs e) {
    // Random-walk utilization with the occasional spike, roughly like the reference graph.
    _utilization += _random.NextDouble() * 20 - 10;
    _utilization = Math.Clamp(_utilization, 0, 100);
    if (_random.NextDouble() < 0.1) {
      _utilization = Math.Min(100, _utilization + 40);
    }
    UtilizationGraph.AddValue(_utilization);

    // Voltage stays essentially flat with tiny jitter, matching the reference graph.
    double voltage = 2.0 - _random.NextDouble() * 0.05;
    VoltageGraph.AddValue(voltage);
    VoltageGaugeView.Value = voltage;
    VoltageBar.Value = voltage;
  }

  private void GraphKindRadio_Changed(object sender, RoutedEventArgs e) {
    // LineModeRadio's IsChecked="True" in XAML fires this Checked event during
    // InitializeComponent(), before the views' templates are applied — bail out rather
    // than null-ref on startup.
    if (UtilizationView?.Graph == null || VoltageView?.Graph == null) return;

    GraphKind kind = SegmentedBarModeRadio.IsChecked == true ? GraphKind.SegmentedBar
        : BarModeRadio.IsChecked == true ? GraphKind.Bar
        : GraphKind.Line;

    UtilizationGraph.Kind = kind;
    VoltageGraph.Kind = kind;

    // Re-apply the theme for the new Kind — the fill brush a theme picks depends on it
    // (gradient for Line, flat solid for Bar/SegmentedBar; see GraphThemes.FromAccent),
    // so switching Kind without this would leave the Line-style gradient fill on bars,
    // where it visibly restarts within every individual bar/segment.
    UtilizationGraph.ApplyTheme(GraphThemes.Rose(kind));
    VoltageGraph.ApplyTheme(GraphThemes.Emerald(kind));
  }
}
