using System.Windows;
using Crystal.NewShell.Navigation;
using Crystal.NewShell.Views;

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
  }

  protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) {
    moduleCatalog.AddModule<CpuModule.CpuModule>();
    moduleCatalog.AddModule<GpuModule.GpuModule>();
    moduleCatalog.AddModule<MemoryModule.MemoryModule>();
    moduleCatalog.AddModule<StorageModule.StorageModule>();
    moduleCatalog.AddModule<BiosModule.BiosModule>();
    moduleCatalog.AddModule<NetworkModule.NetworkModule>();
  }

  protected override void OnInitialized() {
    base.OnInitialized();
    // Show the dashboard once modules have registered their summary tiles.
    Container.Resolve<NavigationController>().NavigateToDashboard();
  }
}
