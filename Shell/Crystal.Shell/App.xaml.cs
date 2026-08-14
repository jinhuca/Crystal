using Crystal.Provider.Etw;
using Crystal.Service.Sensors;
using Crystal.Shell.Navigation;
using Crystal.Shell.Views;
using System.Threading;
using System.Windows;

namespace Crystal.Shell;

/// <summary>
/// Interaction logic for App.xaml. A Prism (Unity) application: it creates the shell
/// window, registers the swappable dashboard, and populates the module catalog. Each
/// module injects its summary tile into the dashboard and registers a full-scale detail
/// view for navigation.
/// </summary>
public partial class App : PrismApplication {
  // Machine-unique names. The app opens ring-0 MSR/driver sessions (PawnIO, the LHM fork), so a
  // second instance would contend for the same hardware handles and double the polling load —
  // instead, a second launch signals the running instance to surface itself, then exits.
  private const string InstanceMutexName = @"Local\Crystal.Shell.SingleInstance";
  private const string ActivateEventName = @"Local\Crystal.Shell.Activate";

  private Mutex? _instanceMutex;
  private EventWaitHandle? _activateSignal;
  private RegisteredWaitHandle? _activateRegistration;

  protected override void OnStartup(StartupEventArgs e) {
    _instanceMutex = new Mutex(initiallyOwned: true, InstanceMutexName, out bool isFirstInstance);

    // The activate signal exists whether we are first or second: the first instance waits on it;
    // a second instance sets it to ask the first to come forward.
    _activateSignal = new EventWaitHandle(false, EventResetMode.AutoReset, ActivateEventName);

    if (!isFirstInstance) {
      // Another instance already owns the mutex — wake it and quit before Prism initializes
      // (base.OnStartup is what opens the container and hardware sessions).
      _activateSignal.Set();
      Shutdown();
      return;
    }

    // First instance: register a background waiter that brings the main window forward whenever
    // a later launch signals. ThreadPool.RegisterWaitForSingleObject fires on a pool thread, so
    // marshal back onto the UI dispatcher before touching windows.
    _activateRegistration = ThreadPool.RegisterWaitForSingleObject(
        _activateSignal, (_, _) => Dispatcher.BeginInvoke(ActivateMainWindow),
        state: null, millisecondsTimeOutInterval: Timeout.Infinite, executeOnlyOnce: false);

    base.OnStartup(e);
  }

  private void ActivateMainWindow() {
    if (MainWindow is not { } window) return;
    if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;
    window.Show();
    window.Activate();
    // Momentary Topmost flip forces the window to the foreground even when the caller isn't the
    // foreground process (Windows otherwise only flashes the taskbar button).
    window.Topmost = true;
    window.Topmost = false;
  }

  protected override void OnExit(ExitEventArgs e) {
    _activateRegistration?.Unregister(null);
    _activateSignal?.Dispose();
    _instanceMutex?.Dispose();
    base.OnExit(e);
  }

  protected override Window CreateShell() => Container.Resolve<Shell>();

  protected override void RegisterTypes(IContainerRegistry containerRegistry) {
    // The dashboard is the shell's default content; register it for navigation so the
    // NavigationController can swap back to it from any detail view.
    containerRegistry.RegisterForNavigation<DashboardView>();

    // Long-lived: resolved eagerly in OnInitialized.
    containerRegistry.RegisterSingleton<NavigationController>();

    // Persists detail-window placement (position/size/pin) across sessions; shared by the
    // window service.
    containerRegistry.RegisterSingleton<WindowLayoutStore>();

    // Long-lived: subscribes to weakly-referenced navigation events (ShowDetail/ShowDashboard),
    // so it must not be collected. Resolved eagerly in OnInitialized.
    containerRegistry.RegisterSingleton<DetailWindowService>();

    // System-wide sensor stream shared by every module that subscribes. SensorMonitor owns the
    // polling lifetime and the hardware session, so it must be a singleton. Built via a factory:
    // its ctor's optional TimeSpan?/IScheduler? params can't be resolved by the container, and we
    // want the default 1-second poll cadence.
    containerRegistry.RegisterSingleton<SensorMonitor>(_ => new SensorMonitor());
  }

  protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) {
    moduleCatalog.AddModule<Crystal.CpuModule.CpuModule>();
    moduleCatalog.AddModule<Crystal.GpuModule.GpuModule>();
    moduleCatalog.AddModule<Crystal.MemoryModule.MemoryModule>();
    moduleCatalog.AddModule<Crystal.StorageModule.StorageModule>();
    moduleCatalog.AddModule<Crystal.BiosModule.BiosModule>();
    moduleCatalog.AddModule<Crystal.NetworkModule.NetworkModule>();
    moduleCatalog.AddModule<Crystal.ProcessModule.ProcessModule>();
    moduleCatalog.AddModule<Crystal.OSModule.OSModule>();
  }

  protected override void OnInitialized() {
    base.OnInitialized();

    // Navigate to the dashboard immediately: each tile is a self-warming LoadingHost (see the
    // modules' OnInitialized) that shows a spinner and warms its own heavy singleton on a
    // background thread, swapping in the real view when ready. The shell no longer blocks on a
    // sequential warm-up, so a slow component (e.g. Storage stalled on disk IO) only delays its
    // own tile instead of the whole dashboard.
    Container.Resolve<NavigationController>().NavigateToDashboard();

    // Eagerly resolve so it starts listening for detail-open requests: it holds only weak
    // event references and would otherwise be collected before any tile is clicked.
    var detailWindows = Container.Resolve<DetailWindowService>();

    // Reopen whatever detail windows were open when the app last exited.
    detailWindows.RestoreSession();

    GateEtwCaptureOnVisibility();
  }

  // Suspend the kernel ETW capture (per-process GPU/Disk/Network) while the window is minimized —
  // its columns aren't visible then, and the continuous per-event accumulation is a notable chunk of
  // idle CPU. The kernel session stays up (avoiding a fragile stop/restart that needs elevation); we
  // just stop feeding its accumulators. Resumed the moment the window is restored.
  private void GateEtwCaptureOnVisibility() {
    if (MainWindow is not { } window) return;
    // ProcessModule registers this; resolve rather than construct so we share the one live session.
    var etw = Container.Resolve<IProcessEtwSource>();

    void Apply() {
      if (window.WindowState == WindowState.Minimized) etw.Pause();
      else etw.Resume();
    }

    window.StateChanged += (_, _) => Apply();
    Apply();
  }
}
