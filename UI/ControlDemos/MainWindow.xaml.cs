using Crystal.Controls;
using Crystal.Controls.Borders;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ControlDemos;

public partial class MainWindow : Window {
  private readonly Stopwatch _clock = Stopwatch.StartNew();
  private int _frames;
  private TimeSpan _lastReport;

  public MainWindow() {
    InitializeComponent();

    TierText.Text = "Render: " + RenderTierInfo.Describe();

    // CompositionTarget.Rendering fires once per composed frame; counting it against a
    // stopwatch gives a live FPS. Note: having a handler keeps the render loop ticking, so an
    // idle scene reads at the monitor refresh — use the stress toggle to make it meaningful.
    CompositionTarget.Rendering += OnRendering;
    Closed += (_, _) => CompositionTarget.Rendering -= OnRendering;
  }

  private void OnRendering(object? sender, EventArgs e) {
    _frames++;
    TimeSpan elapsed = _clock.Elapsed - _lastReport;
    if(elapsed.TotalSeconds >= 0.5) {
      double fps = _frames / elapsed.TotalSeconds;
      FpsText.Text = $"{fps:0} FPS";
      _frames = 0;
      _lastReport = _clock.Elapsed;
    }
  }

  // Oscillate the blur radius to force the effect (and its bitmap cache) to recompute every
  // frame, so the FPS readout reflects the real per-frame cost of the blur.
  private void StressToggle_Checked(object sender, RoutedEventArgs e) {
    var anim = new DoubleAnimation(2, 28, new Duration(TimeSpan.FromSeconds(1))) {
      AutoReverse = true,
      RepeatBehavior = RepeatBehavior.Forever,
    };
    Shadow.BeginAnimation(InnerShadowBorder.ShadowBlurRadiusProperty, anim);
  }

  private void StressToggle_Unchecked(object sender, RoutedEventArgs e) {
    // Clear the animation; the ShadowBlurRadius binding to the slider resumes control.
    Shadow.BeginAnimation(InnerShadowBorder.ShadowBlurRadiusProperty, null);
  }
}
