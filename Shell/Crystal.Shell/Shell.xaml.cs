using Crystal.Shell.Navigation;
using Crystal.Shell.Settings;
using Crystal.Shell.Views;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Crystal.Shell;
/// <summary>
/// Interaction logic for Shell.xaml
/// </summary>
public partial class Shell : Window {
  // Segoe MDL2 Assets glyphs for the maximize (E922) and restore (E923) states.
  private const string MaximizeGlyph = "";
  private const string RestoreGlyph = "";

  // Minimum on-screen extent (px) a restored window must keep so it stays grabbable.
  private const double MinVisible = 120;

  // Persisted under this key in the shared window-layout store (detail windows use their
  // detail-view names, so a fixed key can't collide with them).
  private const string LayoutKey = "MainWindow";

  private readonly WindowLayoutStore _layouts;
  private readonly Settings.GraphSettingsStore _graphSettings;
  private readonly DispatcherTimer _clock;

  // Guards the title-bar graph-shape radios while their initial checked state is being seeded, so
  // reflecting the current selection doesn't itself write the settings back.
  private bool _suppressKindApply;

  public Shell(WindowLayoutStore layouts, Settings.GraphSettingsStore graphSettings) {
    _layouts = layouts;
    _graphSettings = graphSettings;
    InitializeComponent();
    RestorePlacement();
    StateChanged += (_, _) => UpdateMaximizeButton();
    Closing += OnClosing;
    UpdateMaximizeButton();
    InitGraphKindToggle();

    _clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
    _clock.Tick += (_, _) => UpdateClock();
    _clock.Start();
    UpdateClock();
  }

  private void UpdateClock() => DateTimeText.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

  private void OnMinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

  private void OnMaximizeRestoreClick(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized
    ? WindowState.Normal
    : WindowState.Maximized;

  private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

  // Reflect the persisted render mode in the title-bar radios and push it onto the global graph
  // appearance so every AdaptiveGraph builds in the right shape from first paint. Seeding is
  // suppressed so reflecting the current selection doesn't itself re-save it.
  private void InitGraphKindToggle() {
    var mode = _graphSettings.Current.RenderMode;
    Crystal.Controls.PerformanceGraphs.GraphAppearance.Current.Mode = mode;
    _suppressKindApply = true;
    if (mode == Crystal.Controls.PerformanceGraphs.GraphRenderMode.Dot) DotGraphRadio.IsChecked = true;
    else LineGraphRadio.IsChecked = true;
    _suppressKindApply = false;
  }

  private void OnGraphLineChecked(object sender, RoutedEventArgs e) {
    if (!_suppressKindApply) ApplyRenderMode(Crystal.Controls.PerformanceGraphs.GraphRenderMode.Line);
  }

  private void OnGraphDotChecked(object sender, RoutedEventArgs e) {
    if (!_suppressKindApply) ApplyRenderMode(Crystal.Controls.PerformanceGraphs.GraphRenderMode.Dot);
  }

  // Push the chosen render mode onto the global graph appearance (every live AdaptiveGraph rebuilds
  // its inner control immediately) and persist it so the choice is restored on the next launch.
  private void ApplyRenderMode(Crystal.Controls.PerformanceGraphs.GraphRenderMode mode) {
    Crystal.Controls.PerformanceGraphs.GraphAppearance.Current.Mode = mode;
    var current = _graphSettings.Current;
    current.RenderMode = mode;
    _graphSettings.Save(current);
  }

  // Reset the dashboard's resizable rows to their default proportions, and the Processes tile's
  // column widths / master-detail split along with it. Both views are injected into the content
  // region by Prism, so we locate them in the visual tree rather than hold refs.
  private void OnResetLayoutClick(object sender, RoutedEventArgs e) {
    FindDescendant<DashboardView>(MainContent)?.ResetLayout();
    FindDescendant<Crystal.ProcessModule.Views.ProcessSummaryView>(MainContent)?.ResetLayout();
  }

  private static T? FindDescendant<T>(DependencyObject root) where T : DependencyObject {
    if (root is T match) return match;
    int count = VisualTreeHelper.GetChildrenCount(root);
    for (int i = 0; i < count; i++) {
      var found = FindDescendant<T>(VisualTreeHelper.GetChild(root, i));
      if (found is not null) return found;
    }
    return null;
  }

  private void UpdateMaximizeButton() {
    bool maximized = WindowState == WindowState.Maximized;
    MaximizeButton.Content = maximized ? RestoreGlyph : MaximizeGlyph;
  }

  // Restore the last-saved position, size and maximized state. A saved rect is used only if a
  // decent slice still overlaps the current virtual desktop (a monitor may have been unplugged).
  private void RestorePlacement() {
    var saved = _layouts.Get(LayoutKey);
    if (saved is not { HasBounds: true }) return;

    if (IsOnScreen(saved)) {
      WindowStartupLocation = WindowStartupLocation.Manual;
      Left = saved.Left;
      Top = saved.Top;
      Width = saved.Width;
      Height = saved.Height;
    }

    if (saved.Maximized) WindowState = WindowState.Maximized;
  }

  private void OnClosing(object? sender, CancelEventArgs e) {
    // RestoreBounds holds the normal-state rect even when maximized, so "restore down" returns to
    // the right place next launch; fall back to the live bounds if it's empty.
    var r = RestoreBounds;
    bool hasRect = !r.IsEmpty && r.Width > 0 && r.Height > 0;
    _layouts.Save(LayoutKey, new WindowLayout {
      Left = hasRect ? r.Left : Left,
      Top = hasRect ? r.Top : Top,
      Width = hasRect ? r.Width : Width,
      Height = hasRect ? r.Height : Height,
      Maximized = WindowState == WindowState.Maximized,
      HasBounds = true,
    });
  }

  private static bool IsOnScreen(WindowLayout saved) {
    double vLeft = SystemParameters.VirtualScreenLeft;
    double vTop = SystemParameters.VirtualScreenTop;
    double vRight = vLeft + SystemParameters.VirtualScreenWidth;
    double vBottom = vTop + SystemParameters.VirtualScreenHeight;

    double visibleWidth = System.Math.Min(saved.Left + saved.Width, vRight)
                          - System.Math.Max(saved.Left, vLeft);
    double visibleHeight = System.Math.Min(saved.Top + saved.Height, vBottom)
                           - System.Math.Max(saved.Top, vTop);

    bool titleBarReachable = saved.Top >= vTop && saved.Top <= vBottom - MinVisible;
    return titleBarReachable && visibleWidth >= MinVisible && visibleHeight >= MinVisible;
  }
}
