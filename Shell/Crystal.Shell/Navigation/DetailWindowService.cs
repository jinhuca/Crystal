using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using Crystal.Infrastructure.Constants.Navigation;

namespace Crystal.Shell.Navigation;

/// <summary>
/// Hosts module detail views in modeless top-level windows instead of swapping them into
/// the shell's content region, so the dashboard stays visible and several subsystems can be
/// watched side by side (e.g. on a second monitor). Each window can be pinned always-on-top,
/// and its placement (position, size, pin state) is persisted per subsystem via
/// <see cref="WindowLayoutStore"/> so reopening restores where the user left it.
/// <para>
/// A long-lived singleton: it subscribes to the weakly-referenced navigation events, so a
/// transient would be collected and stop responding — resolved eagerly at startup, same as
/// <see cref="NavigationController"/>.
/// </para>
/// </summary>
public sealed class DetailWindowService {
  // Each detail view is registered for navigation under a DetailViewNames string; the shell
  // references every module project, so we map those names to their view types here rather
  // than threading a registration API through Constants.
  private static readonly IReadOnlyDictionary<string, System.Type> ViewTypes =
      new Dictionary<string, System.Type> {
        [DetailViewNames.Cpu] = typeof(Crystal.CpuModule.Views.CpuDetailView),
        [DetailViewNames.Gpu] = typeof(Crystal.GpuModule.Views.GpuDetailView),
        [DetailViewNames.Memory] = typeof(Crystal.MemoryModule.Views.MemoryDetailView),
        [DetailViewNames.Storage] = typeof(Crystal.StorageModule.Views.StorageDetailView),
        [DetailViewNames.Bios] = typeof(Crystal.BiosModule.Views.BiosDetailView),
        [DetailViewNames.Network] = typeof(Crystal.NetworkModule.Views.NetworkDetailView),
        [DetailViewNames.Os] = typeof(Crystal.OSModule.Views.OsDetailView),
      };

  private const double DefaultWidth = 1280;
  private const double DefaultHeight = 820;

  // Small diagonal offset per successive window so multiples don't stack exactly.
  private const double CascadeStep = 28;

  // Minimum on-screen extent (px) a restored window must keep so it stays grabbable.
  private const double MinVisible = 120;

  private readonly IContainerProvider _container;
  private readonly WindowLayoutStore _layouts;

  // One live window per detail name — a second request re-focuses instead of duplicating.
  private readonly Dictionary<string, Window> _open = new();

  // Set once the app is closing. WPF fires every modeless window's Closed during shutdown, and
  // we must NOT clear their persisted Open flag then — that flag is exactly the session-restore
  // set we want to reopen next launch. Only a user-initiated close (flag still false) clears it.
  private bool _shuttingDown;

  public DetailWindowService(IContainerProvider container, WindowLayoutStore layouts,
                             IEventAggregator events) {
    _container = container;
    _layouts = layouts;
    events.GetEvent<ShowDetailEvent>().Subscribe(Show);
    events.GetEvent<ShowDashboardEvent>().Subscribe(CloseActive);

    // MainWindow.Closing fires before shutdown closes the detail windows, so it's our signal to
    // preserve the open-set. The service is resolved after the shell exists, so MainWindow is set.
    if (Application.Current?.MainWindow is { } main)
      main.Closing += (_, _) => _shuttingDown = true;
  }

  /// <summary>Reopens every detail window that was open when the app last exited.</summary>
  public void RestoreSession() {
    foreach (var detailViewName in _layouts.OpenKeys())
      Show(detailViewName);
  }

  private void Show(string detailViewName) {
    if (_open.TryGetValue(detailViewName, out var existing)) {
      if (existing.WindowState == WindowState.Minimized) existing.WindowState = WindowState.Normal;
      existing.Activate();
      return;
    }

    if (!ViewTypes.TryGetValue(detailViewName, out var viewType)) return;

    // Resolve through the container so the detail view gets its own (transient) view model,
    // keeping its live graph buffers independent of the dashboard tile's.
    var content = (FrameworkElement)_container.Resolve(viewType);
    var saved = _layouts.Get(detailViewName);

    var window = new Window {
      Title = "Crystal — " + TitleFor(detailViewName),
      Background = Brushes.Black,
      Width = saved is { HasBounds: true } ? saved.Width : DefaultWidth,
      Height = saved is { HasBounds: true } ? saved.Height : DefaultHeight,
      Topmost = saved?.Topmost ?? false,
      Owner = Application.Current?.MainWindow,
    };
    window.Content = BuildChrome(detailViewName, content, saved?.Topmost ?? false, window);

    PlaceWindow(window, saved);

    window.Closed += (_, _) => {
      _open.Remove(detailViewName);
      // A user closing this window drops it from the session-restore set; app shutdown leaves
      // the flag set so the window reopens next launch.
      PersistLayout(detailViewName, window, open: _shuttingDown);
      // The detail view models unsubscribe their sensor streams on Dispose; releasing here
      // stops a closed window's graphs from being fed for the life of the app.
      (content.DataContext as System.IDisposable)?.Dispose();
    };

    _open[detailViewName] = window;
    // Record it as part of the open-set immediately, so a crash/kill still restores it.
    PersistLayout(detailViewName, window, open: true);
    window.Show();
  }

  // Detail title bar: slightly slimmer than the dashboard's 43px so a detail window reads as a
  // sibling of the dashboard, not a clone.
  private const double TitleBarHeight = 36;

  // Segoe MDL2 Assets caption glyphs, matching the dashboard's caption buttons.
  private const string MinimizeGlyph = "";
  private const string MaximizeGlyph = "";
  private const string RestoreGlyph = "";
  private const string CloseGlyph = "";

  private static readonly SolidColorBrush AccentBrush = new(Color.FromRgb(0x3E, 0x9B, 0xE8));
  private static readonly SolidColorBrush BrandBrush = new(Color.FromRgb(0x8A, 0x94, 0xA0));

  // Gives the detail window the dashboard's custom chrome instead of the native OS title bar: a
  // WindowChrome strips the OS frame, an accent border matches the dashboard, and we draw our own
  // title bar carrying the brand, the always-on-top pin, and minimize/maximize/close caption
  // buttons. The pin persists immediately so the preference survives even if the window is never
  // moved or resized.
  private object BuildChrome(string detailViewName, FrameworkElement content, bool pinned, Window window) {
    WindowChrome.SetWindowChrome(window, new WindowChrome {
      CaptionHeight = TitleBarHeight,
      GlassFrameThickness = new Thickness(0),
      CornerRadius = new CornerRadius(0),
      ResizeBorderThickness = new Thickness(6),
      UseAeroCaptionButtons = false,
    });
    window.BorderBrush = AccentBrush;
    window.BorderThickness = new Thickness(1);

    var app = Application.Current;

    var pin = new ToggleButton {
      Content = "\U0001F4CC Pin",
      IsChecked = pinned,
      Margin = new Thickness(0, 6, 6, 6),
      VerticalAlignment = VerticalAlignment.Center,
      Style = app?.TryFindResource("DetailToolbarToggleStyle") as Style,
      ToolTip = "Keep this window above other windows",
    };
    WindowChrome.SetIsHitTestVisibleInChrome(pin, true);
    pin.Checked += (_, _) => SetTopmost(detailViewName, true);
    pin.Unchecked += (_, _) => SetTopmost(detailViewName, false);

    var captionStyle = app?.TryFindResource("DetailCaptionButtonStyle") as Style;
    var closeStyle = app?.TryFindResource("DetailCloseButtonStyle") as Style;

    var minimize = new Button { Content = MinimizeGlyph, Style = captionStyle, ToolTip = "Minimize" };
    minimize.Click += (_, _) => window.WindowState = WindowState.Minimized;

    var maximize = new Button {
      Content = window.WindowState == WindowState.Maximized ? RestoreGlyph : MaximizeGlyph,
      Style = captionStyle, ToolTip = "Maximize",
    };
    maximize.Click += (_, _) => window.WindowState =
        window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    window.StateChanged += (_, _) =>
        maximize.Content = window.WindowState == WindowState.Maximized ? RestoreGlyph : MaximizeGlyph;

    var close = new Button { Content = CloseGlyph, Style = closeStyle, ToolTip = "Close" };
    close.Click += (_, _) => window.Close();

    var right = new StackPanel {
      Orientation = Orientation.Horizontal,
      HorizontalAlignment = HorizontalAlignment.Right,
      VerticalAlignment = VerticalAlignment.Stretch,
      Children = { pin, minimize, maximize, close },
    };

    // Brand: logo + "Crystal — <Subsystem>", with the subsystem accented so the detail window is
    // instantly distinguishable from the dashboard's plain muted title — the intentional
    // "slight difference".
    var brand = new StackPanel {
      Orientation = Orientation.Horizontal,
      HorizontalAlignment = HorizontalAlignment.Left,
      VerticalAlignment = VerticalAlignment.Center,
      Margin = new Thickness(10, 0, 0, 0),
    };
    if (TryLoadLogo() is { } logo)
      brand.Children.Add(new Image {
        Source = logo, Width = 18, Height = 18, Margin = new Thickness(0, 0, 8, 0),
        VerticalAlignment = VerticalAlignment.Center,
      });
    var title = new TextBlock {
      VerticalAlignment = VerticalAlignment.Center,
      FontFamily = new FontFamily("Segoe UI"),
      FontSize = 14,
    };
    title.Inlines.Add(new Run("Crystal ") { Foreground = BrandBrush });
    title.Inlines.Add(new Run("— " + TitleFor(detailViewName)) { Foreground = AccentBrush });
    brand.Children.Add(title);

    var titleBar = new Grid { Height = TitleBarHeight, Background = Brushes.Black };
    titleBar.Children.Add(brand);
    titleBar.Children.Add(right);
    Grid.SetRow(titleBar, 0);
    Grid.SetRow(content, 1);

    var root = new Grid();
    root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(TitleBarHeight) });
    root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
    root.Children.Add(titleBar);
    root.Children.Add(content);
    return root;
  }

  // Best-effort load of the app icon for the title bar; a missing/failed resource just drops the
  // logo rather than taking the window down.
  private static ImageSource? TryLoadLogo() {
    try {
      return new BitmapImage(new System.Uri("pack://application:,,,/Crystal.ico"));
    } catch {
      return null;
    }
  }

  private void SetTopmost(string detailViewName, bool topmost) {
    if (!_open.TryGetValue(detailViewName, out var window)) return;
    window.Topmost = topmost;
    // The window is live (it's in _open), so it stays part of the open-set.
    PersistLayout(detailViewName, window, open: true);
  }

  private void PlaceWindow(Window window, WindowLayout? saved) {
    if (saved is { HasBounds: true } && IsOnScreen(saved)) {
      window.WindowStartupLocation = WindowStartupLocation.Manual;
      window.Left = saved.Left;
      window.Top = saved.Top;
      return;
    }

    // No saved bounds (or the saved spot is off-screen — e.g. a monitor was unplugged since):
    // center the first window, cascade any later ones off the main window.
    int offset = _open.Count;
    if (offset == 0) {
      window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      return;
    }
    window.WindowStartupLocation = WindowStartupLocation.Manual;
    var owner = window.Owner;
    window.Left = (owner?.Left ?? 0) + CascadeStep * offset;
    window.Top = (owner?.Top ?? 0) + CascadeStep * offset;
  }

  // A saved rect is usable only if a decent slice of it still overlaps the current virtual
  // desktop — the desktop shrinks when a monitor is unplugged, stranding windows that were on it.
  // Requiring both a minimum-visible width/height and the title bar to be reachable keeps a
  // restored window grabbable rather than opening it fully or partly off-screen.
  private static bool IsOnScreen(WindowLayout saved) {
    double vLeft = SystemParameters.VirtualScreenLeft;
    double vTop = SystemParameters.VirtualScreenTop;
    double vRight = vLeft + SystemParameters.VirtualScreenWidth;
    double vBottom = vTop + SystemParameters.VirtualScreenHeight;

    double visibleWidth = System.Math.Min(saved.Left + saved.Width, vRight)
                          - System.Math.Max(saved.Left, vLeft);
    double visibleHeight = System.Math.Min(saved.Top + saved.Height, vBottom)
                           - System.Math.Max(saved.Top, vTop);

    // The title bar (top edge) must sit within the desktop, else it can't be dragged back.
    bool titleBarReachable = saved.Top >= vTop && saved.Top <= vBottom - MinVisible;
    return titleBarReachable && visibleWidth >= MinVisible && visibleHeight >= MinVisible;
  }

  private void PersistLayout(string detailViewName, Window window, bool open) {
    // RestoreBounds holds the normal-state rect even when minimized/maximized; fall back to the
    // live Left/Top/Width/Height when it is empty (window still in its startup placement).
    var r = window.RestoreBounds;
    bool hasRect = !r.IsEmpty && r.Width > 0 && r.Height > 0;
    _layouts.Save(detailViewName, new WindowLayout {
      Left = hasRect ? r.Left : window.Left,
      Top = hasRect ? r.Top : window.Top,
      Width = hasRect ? r.Width : window.Width,
      Height = hasRect ? r.Height : window.Height,
      Topmost = window.Topmost,
      HasBounds = true,
      Open = open,
    });
  }

  // "Back" on a detail view publishes ShowDashboardEvent; with windowed detail that means
  // "close this detail window". Close the currently-active one if it is one of ours.
  private void CloseActive() {
    foreach (var window in _open.Values) {
      if (window.IsActive) { window.Close(); return; }
    }
  }

  private static string TitleFor(string detailViewName) =>
      detailViewName.EndsWith("DetailView", System.StringComparison.Ordinal)
          ? detailViewName[..^"DetailView".Length]
          : detailViewName;
}
