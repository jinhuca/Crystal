using Crystal.Infrastructure.Constants;
using Crystal.Infrastructure.Constants.Navigation;
using NetworkModule.Models;
using NetworkModule.ViewModels;
using NetworkModule.Views;

namespace NetworkModule;

/// <summary>
/// Prism module for Network (live activity only, no static inventory). Registers the
/// source→model→view-model chain, injects the compact <see cref="NetworkSummaryView"/> into the
/// dashboard's network tile region, and registers the full-scale <see cref="NetworkDetailView"/>
/// for navigation.
/// </summary>
public class NetworkModule(IRegionManager regionManager) : IModule {
  private readonly IRegionManager _regionManager = regionManager;

  public void RegisterTypes(IContainerRegistry containerRegistry) {
    // NetworkLoadSource owns an open LibreHardwareMonitor Computer; keep one for the app lifetime.
    containerRegistry.RegisterSingleton<NetworkLoadSource>();

    // NetworkModel owns the load-polling lifetime, so it must be a singleton. Built via a factory:
    // its ctor's optional TimeSpan?/IScheduler? params can't be resolved by the container, and we
    // want the default 1-second poll cadence.
    containerRegistry.RegisterSingleton<INetworkModel>(cp => new NetworkModel(cp.Resolve<NetworkLoadSource>()));

    // Each view gets its own VM instance (Register, not singleton) so summary and detail never
    // share adapter graph buffers.
    containerRegistry.Register<INetworkViewModel, NetworkViewModel>();

    containerRegistry.RegisterForNavigation<NetworkDetailView>(DetailViewNames.Network);

    ViewModelLocationProvider.Register<NetworkSummaryView>(
        () => ContainerLocator.Container.Resolve<INetworkViewModel>());
    ViewModelLocationProvider.Register<NetworkDetailView>(
        () => ContainerLocator.Container.Resolve<INetworkViewModel>());
  }

  public void OnInitialized(IContainerProvider containerProvider) {
    _regionManager.RegisterViewWithRegion(RegionNames.NetworkSummaryRegionName, typeof(NetworkSummaryView));
  }
}
