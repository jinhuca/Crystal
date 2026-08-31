using Crystal.Controls.Demo.Support;
using Crystal.Controls.Loading;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;
using Crystal.Controls.RangeBars.Themes;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Crystal.Controls.Demo;

/// <summary>
/// Every control and style in Crystal.Controls, in one window - replaces the earlier
/// MainWindow/GraphLiteWindow/MultipleDSWindow split.
/// </summary>
public partial class AllControlsWindow : Window {
  // One RandomWalk per independently-varying series. Separate instances (rather than one shared
  // walk fed to everything) so tiles meant to represent different sensors don't move in lockstep -
  // the four GraphKind-comparison tiles are the one deliberate exception, since the whole point of
  // that row is comparing how identical data looks under each Kind.
  private readonly RandomWalk _utilization = new(0, 100, start: 35, seed: 1);
  private readonly RandomWalk _voltage = new(0, 3, start: 1.2, maxStep: 0.05, seed: 2);
  private readonly RandomWalk _load = new(0, 100, start: 40, seed: 3);
  private readonly RandomWalk _mem = new(0, 100, start: 60, seed: 4);
  private readonly RandomWalk _core0 = new(0, 100, start: 20, seed: 11);
  private readonly RandomWalk _core1 = new(0, 100, start: 55, seed: 12);
  private readonly RandomWalk _core2 = new(0, 100, start: 70, seed: 13);

  // Section E2: one value sweeps the full 0-100 range with a larger step so the value-banded
  // fill visibly steps green→yellow→orange→red as it crosses each 20-wide gate.
  private readonly RandomWalk _banded = new(0, 100, start: 10, maxStep: 9, seed: 21);

  // Section C3 (PerformanceGraphMultipleDS): independent from _utilization/_load/etc. above so
  // this section's three lines don't move in lockstep with anything else on the page.
  private readonly RandomWalk _cpuWalk = new(0, 100, start: 35, seed: 101);
  private readonly RandomWalk _ramWalk = new(0, 100, start: 50, seed: 102);
  private readonly RandomWalk _gpuWalk = new(0, 100, start: 20, seed: 103);
  private readonly ObservableCollection<double> _gpuSamples = new();

  // The eighth Section-B tile: built in code-behind (not XAML) so it can use the
  // (int capacity) constructor overload - a plain XAML element always calls the parameterless one.
  private readonly PerformanceGraphLite _liteCustomCapacity;

  // Backs the ValuesSource-bound tile (LiteBound) - see LiteValuesSourceDemo's own doc comment.
  private readonly LiteValuesSourceDemo _liteValuesSourceDemo = new();

  private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

  public AllControlsWindow() {
    InitializeComponent();

    // --- Section A: PerformanceGraph is a plain FrameworkElement, not a templated Control, so
    // each x:Name is usable immediately here - no ApplyTemplate() dance needed (contrast Sections
    // C/D below). Kind/MinValue/MaxValue/HistoryLength are already set in XAML; ApplyTheme
    // supplies the rest (background/grid/border/line/fill) so the four tiles are colored
    // consistently with the rest of this app's theme choices.
    KindLineGraph.ApplyTheme(GraphThemes.Rose(GraphKind.Line));
    KindBarGraph.ApplyTheme(GraphThemes.Amber(GraphKind.Bar));
    KindSegGraph.ApplyTheme(GraphThemes.Emerald(GraphKind.SegmentedBar));
    KindDotGraph.ApplyTheme(GraphThemes.Sky(GraphKind.Dot));

    // LiteBound's ValuesSource binding ({Binding UtilizationSamples} in XAML) resolves against
    // whatever DataContext it inherits - set it here, on the element itself, rather than on the
    // Window, so this one demo view-model doesn't leak into every other tile's DataContext.
    LiteBound.DataContext = _liteValuesSourceDemo;

    // --- Section B: the eighth tile, added programmatically to demonstrate the ctor overload.
    _liteCustomCapacity = new PerformanceGraphLite(capacity: 30) {
      Height = 120,
      MinValue = 0,
      MaxValue = 100,
      Flip = true,
      // SingleColor + DotColor is the real feature, rather than the old workaround of setting
      // every band color to the same brush - this tile isolates Flip as the one thing being shown
      // here, instead of showing two features tangled together, and does it through the
      // one-draw-call render path rather than the nine-geometry Banded one.
      ColorMode = DotColorMode.SingleColor,
      DotColor = Brushes.MediumPurple,
    };
    LiteRow.Children.Add(new Border {
      Style = (Style)FindResource("TileBorderStyle"),
      Margin = new Thickness(0),
      Child = new StackPanel {
        Children = {
          new TextBlock {
            Text = "PerformanceGraphLite (capacity: 30, Flip, SingleColor)",
            Style = (Style)FindResource("TileCaptionStyle"),
          },
          _liteCustomCapacity,
        },
      },
    });

    // --- Section C: PerformanceGraphView is a templated Control - its Graph is null until the
    // template has actually applied, which normally happens during the first layout pass, not
    // immediately after construction. Force it now so the theme lands before the first paint.
    CpuUtilView.ApplyTemplate();
    CpuUtilView.Graph!.ApplyTheme(GraphThemes.Rose(GraphKind.Line));

    // --- Section C3: GpuSeries's ValuesSource is assigned here in code-behind rather than a XAML
    // {Binding} - DataSeries is a plain DependencyObject, not a FrameworkElement, so it has no
    // DataContext to resolve a {Binding} against (see DataSeries.ValuesSource's own doc comment).
    GpuSeries.ValuesSource = _gpuSamples;

    // --- Section D: same ApplyTemplate timing requirement, this time for RangeBarView.Bar.
    VoltageBarView.ApplyTemplate();
    VoltageBarView.Bar!.ApplyTheme(RangeBarThemes.Sky());
    LoadBarView.ApplyTemplate();
    LoadBarView.Bar!.ApplyTheme(RangeBarThemes.Emerald());
    MemBarView.ApplyTemplate();
    MemBarView.Bar!.ApplyTheme(RangeBarThemes.Amber());

    // --- Section G: kick off both LoadingHost tiles now; the Restart button repeats this.
    BeginLoadingDemo();

    _timer.Tick += Timer_Tick;
    _timer.Start();
  }

  private void Timer_Tick(object? sender, EventArgs e) {
    // Section A + six of Section B's tiles (all but LiteBound) plot the same value via AddValue,
    // on purpose - that's what makes them a fair side-by-side comparison of rendering approach
    // rather than of data. LiteBound gets the identical value fed through the
    // ObservableCollection<double> below instead, so it's plotting the same series too.
    double util = _utilization.Next();
    KindLineGraph.AddValue(util);
    KindBarGraph.AddValue(util);
    KindSegGraph.AddValue(util);
    KindDotGraph.AddValue(util);

    NoFrillsDotGraph.AddValue(util);
    LiteDefault.AddValue(util);
    LiteSingleColor.AddValue(util);
    LiteStyled.AddValue(util);
    LiteRounded.AddValue(util);
    LiteThemed.AddValue(util);
    _liteCustomCapacity.AddValue(util);

    // LiteBound never gets an AddValue call anywhere in this file - ValuesSource's own
    // CollectionChanged subscription is what carries this Add into the graph's ring buffer.
    // Capping via RemoveAt(0) once the collection outgrows the graph's own Capacity is this
    // consumer's responsibility, not ValuesSource's - see LiteValuesSourceDemo's doc comment.
    var boundSamples = _liteValuesSourceDemo.UtilizationSamples;
    boundSamples.Add(util);
    if (boundSamples.Count > LiteBound.Capacity) boundSamples.RemoveAt(0);

    CpuUtilView.Graph!.AddValue(util);

    // SquareGridGraph is here to demonstrate the grid staying square as the window resizes, not to
    // demonstrate a second independent data feed - reusing the same utilization value keeps that
    // the whole point rather than adding yet another unrelated RandomWalk.
    SquareGridGraph.AddValue(util);

    // Section C3 (PerformanceGraphMultipleDS): same shared MinValue/MaxValue/Capacity axis for
    // all three, so this is a fair comparison of "how the data got there" (AddValue vs.
    // ValuesSource) rather than of the data itself.
    CpuSeries.AddValue(_cpuWalk.Next());
    RamSeries.AddValue(_ramWalk.Next());

    // GpuSeries never gets an AddValue call - its ValuesSource subscription (wired in the
    // constructor) is what carries this Add into its buffer. ValuesSource only ever appends into
    // the graph's ring buffer, it never shrinks the source collection, so capping it here is this
    // consumer's own responsibility.
    _gpuSamples.Add(_gpuWalk.Next());
    if (_gpuSamples.Count > MultiGraph.Capacity) _gpuSamples.RemoveAt(0);

    // Through the View's own Value property, not .Bar.Value directly - the default template
    // TemplateBinds the bar's Value to RangeBarView.Value AND drives the header's value label
    // from the same property. Setting .Bar.Value directly would move the fill but leave that
    // label stuck at its default forever.
    VoltageBarView.Value = _voltage.Next();
    LoadBarView.Value = _load.Next();
    MemBarView.Value = _mem.Next();

    Core0Bar.Value = _core0.Next();
    Core1Bar.Value = _core1.Next();
    Core2Bar.Value = _core2.Next();

    // Section E2: same value into both bars; the converter, not this code, chooses the colour.
    double banded = _banded.Next();
    BandedRangeBar.Value = banded;
    BandedSegBar.Value = banded;
  }

  // Demonstrates LoadingHost.Begin: LoadingA succeeds after a short simulated warm-up; LoadingB
  // deliberately throws during warm-up to show the Failed marker instead of blocking or crashing.
  private void BeginLoadingDemo() {
    LoadingA.State = LoadingState.Loading;
    LoadingA.Begin(
        warm: () => Task.Delay(TimeSpan.FromSeconds(1.5)).GetAwaiter().GetResult(),
        createContent: () => new TextBlock {
          Text = "Sensors ready",
          Foreground = Brushes.White,
          HorizontalAlignment = HorizontalAlignment.Center,
          VerticalAlignment = VerticalAlignment.Center,
        });

    LoadingB.State = LoadingState.Loading;
    LoadingB.Begin(
        warm: () => throw new InvalidOperationException("Simulated Storage warm-up failure."),
        createContent: () => new TextBlock { Text = "unreachable" });
  }

  private void RestartLoadingButton_Click(object sender, RoutedEventArgs e) => BeginLoadingDemo();
}
