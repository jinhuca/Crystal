using System.Windows;
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
  protected override Window CreateShell() => Container.Resolve<MainWindow>();

  protected override void RegisterTypes(IContainerRegistry containerRegistry) {
    // The dashboard is the shell's default content; register it for navigation so the
    // NavigationController can swap back to it from any detail view.
    containerRegistry.RegisterForNavigation<DashboardView>();

    // Long-lived: subscribes to weakly-referenced navigation events, so it must not be
    // collected. Resolved eagerly in OnInitialized.
    containerRegistry.RegisterSingleton<NavigationController>();

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
  }
}
