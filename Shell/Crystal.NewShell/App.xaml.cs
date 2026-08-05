using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Crystal.Infrastructure.Constants;
using Crystal.NewShell.Navigation;
using Crystal.NewShell.Startup;
using Crystal.NewShell.Views;
using Crystal.Service.Sensors;

namespace Crystal.NewShell;

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
  private const string InstanceMutexName = @"Local\Crystal.NewShell.SingleInstance";
  private const string ActivateEventName = @"Local\Crystal.NewShell.Activate";

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

  protected override Window CreateShell() => Container.Resolve<MainWindow>();

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

    // Warms the heavy module singletons off the UI thread behind a loading screen (see
    // OnInitialized), so opening their ring-0 hardware sessions no longer freezes startup.
    containerRegistry.RegisterSingleton<StartupLoader>();
  }

  protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) {
    moduleCatalog.AddModule<CpuModule.CpuModule>();
    moduleCatalog.AddModule<GpuModule.GpuModule>();
    moduleCatalog.AddModule<MemoryModule.MemoryModule>();
    moduleCatalog.AddModule<StorageModule.StorageModule>();
    moduleCatalog.AddModule<BiosModule.BiosModule>();
    moduleCatalog.AddModule<NetworkModule.NetworkModule>();
    moduleCatalog.AddModule<ProcessModule.ProcessModule>();
  }

  protected override async void OnInitialized() {
    base.OnInitialized();

    // The shell window is already visible at this point. Building the dashboard resolves every
    // module's singleton model, and each opens a ring-0 hardware session in its constructor -
    // doing that synchronously on the UI thread froze the window at startup. Instead: show a
    // lightweight loading overlay now, warm those singletons on a background thread (reporting
    // per-component progress), then swap in the dashboard, whose tiles reuse the warmed instances.
    var regionManager = Container.Resolve<IRegionManager>();
    var loader = Container.Resolve<StartupLoader>();

    var loadingViewModel = new LoadingViewModel(loader.ComponentNames);
    var loadingView = new LoadingView { DataContext = loadingViewModel };
    regionManager.Regions[RegionNames.MainContentRegionName].Add(loadingView);

    // Progress<T> captures the UI SynchronizationContext, so Report runs back on the UI thread.
    var progress = new Progress<StartupProgress>(loadingViewModel.Report);
    await loader.WarmUpAsync(progress);

    // Warmed and back on the UI thread: swap the overlay for the dashboard.
    regionManager.Regions[RegionNames.MainContentRegionName].Remove(loadingView);
    Container.Resolve<NavigationController>().NavigateToDashboard();

    // Eagerly resolve so it starts listening for detail-open requests: it holds only weak
    // event references and would otherwise be collected before any tile is clicked.
    var detailWindows = Container.Resolve<DetailWindowService>();

    // Reopen whatever detail windows were open when the app last exited.
    detailWindows.RestoreSession();
  }
}
