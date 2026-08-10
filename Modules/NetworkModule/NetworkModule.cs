using Crystal.Controls.Loading;
using Crystal.Infrastructure.Constants;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.Provider.Etw;
using Crystal.Provider.Telemetry.Hardware.Network;
using Crystal.Service.Network;
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
    // Wi-Fi radio state source (wlanapi). NetworkLoadSource depends on the interface so it can be
    // faked in tests.
    containerRegistry.RegisterSingleton<IWlanSource, WlanSource>();

    // NetworkLoadSource owns an open LibreHardwareMonitor Computer; keep one for the app lifetime.
    containerRegistry.RegisterSingleton<NetworkLoadSource>();

    // Per-process network top-talkers, driven by the shared ETW broadcaster ProcessModule registers
    // (a singleton in the shared container). Built via a factory for its optional name-resolver param.
    containerRegistry.RegisterSingleton<ProcessNetworkSource>(
        cp => new ProcessNetworkSource(cp.Resolve<EtwRateBroadcaster>()));

    // NetworkMonitor owns the load-polling lifetime, so it must be a singleton. Built via a factory:
    // its ctor's optional TimeSpan?/IScheduler? params can't be resolved by the container, and we
    // want the default 1-second poll cadence.
    containerRegistry.RegisterSingleton<NetworkMonitor>(
        cp => new NetworkMonitor(cp.Resolve<NetworkLoadSource>(), cp.Resolve<ProcessNetworkSource>()));
    containerRegistry.RegisterSingleton<INetworkModel, NetworkModel>();

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
    // Self-warming loading tile: spinner now, warm the model singleton off the UI thread, swap in
    // the real view when ready. See CpuModule for the rationale.
    _regionManager.RegisterViewWithRegion(RegionNames.NetworkSummaryRegionName, () => {
      var host = new LoadingHost { Label = "Network" };
      host.Begin(
          () => containerProvider.Resolve<INetworkModel>(),
          () => new NetworkSummaryView());
      return host;
    });
  }
}
