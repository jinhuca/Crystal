using Crystal.Controls.Loading;
using Crystal.Infrastructure.Constants;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.OSModule.Models;
using Crystal.OSModule.ViewModels;
using Crystal.OSModule.Views;

namespace Crystal.OSModule;

/// <summary>
/// Prism module for the operating system. Registers the builder→model→view-model chain, injects the
/// compact <see cref="OsSummaryView"/> into the dashboard's OS region, and registers the full-scale
/// <see cref="OsDetailView"/> for navigation. Follows MemoryModule: a one-shot replayed identity
/// build plus a ref-counted live poll.
/// </summary>
public class OSModule(IRegionManager regionManager) : IModule {
  private readonly IRegionManager _regionManager = regionManager;

  public void RegisterTypes(IContainerRegistry containerRegistry) {
    containerRegistry.Register<OsInfoBuilder>();

    // OsModel replays its one-shot identity build and owns the live-polling lifetime, so it must be
    // a singleton. Built via a factory: its ctor's optional TimeSpan?/IScheduler?/clock params can't
    // be resolved by the container, and we want the default 1-second poll cadence.
    containerRegistry.RegisterSingleton<IOsModel>(cp => new OsModel(cp.Resolve<OsInfoBuilder>()));

    containerRegistry.Register<IOsViewModel, OsViewModel>();

    containerRegistry.RegisterForNavigation<OsDetailView>(DetailViewNames.Os);

    ViewModelLocationProvider.Register<OsSummaryView>(
        () => ContainerLocator.Container.Resolve<IOsViewModel>());
    ViewModelLocationProvider.Register<OsDetailView>(
        () => ContainerLocator.Container.Resolve<IOsViewModel>());
  }

  public void OnInitialized(IContainerProvider containerProvider) {
    // Self-warming loading tile: spinner now, warm the model singleton off the UI thread, swap in
    // the real view when ready. See CpuModule for the rationale.
    _regionManager.RegisterViewWithRegion(RegionNames.OsRegionName, () => {
      var host = new LoadingHost { Label = "Operating System" };
      host.Begin(
          () => containerProvider.Resolve<IOsModel>(),
          () => new OsSummaryView());
      return host;
    });
  }
}
