using System.ComponentModel;
using System.Windows;
using Crystal.Shell.Navigation;

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

  public Shell(WindowLayoutStore layouts) {
    _layouts = layouts;
    InitializeComponent();
    RestorePlacement();
    StateChanged += (_, _) => UpdateMaximizeButton();
    Closing += OnClosing;
    UpdateMaximizeButton();
  }

  private void OnMinimizeClick(object sender, RoutedEventArgs e) =>
      WindowState = WindowState.Minimized;

  private void OnMaximizeRestoreClick(object sender, RoutedEventArgs e) =>
      WindowState = WindowState == WindowState.Maximized
          ? WindowState.Normal
          : WindowState.Maximized;

  private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

  private void UpdateMaximizeButton() {
    bool maximized = WindowState == WindowState.Maximized;
    MaximizeButton.Content = maximized ? RestoreGlyph : MaximizeGlyph;
    MaximizeButton.ToolTip = maximized ? "Restore" : "Maximize";
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
