using Crystal.Controls.Loading;
using Crystal.Infrastructure.Constants;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Service.Storage;
using StorageModule.Models;
using StorageModule.ViewModels;
using StorageModule.Views;

namespace StorageModule;

/// <summary>
/// Prism module for Storage. Registers the provider→builder→model→view-model chain (static
/// inventory plus live disk activity), injects the compact <see cref="StorageSummaryView"/> into
/// the dashboard's storage tile region, and registers the full-scale <see cref="StorageDetailView"/>
/// for navigation.
/// </summary>
public class StorageModule(IRegionManager regionManager) : IModule {
  private readonly IRegionManager _regionManager = regionManager;

  public void RegisterTypes(IContainerRegistry containerRegistry) {
    containerRegistry.Register<IWmiHardwareProvider, WmiHardwareProvider>();
    containerRegistry.Register<StorageInfoBuilder>();

    // StorageLoadSource owns an open LibreHardwareMonitor Computer; keep one for the app lifetime.
    containerRegistry.RegisterSingleton<StorageLoadSource>();

    // StorageMonitor replays its one-shot spec build and owns the load-polling lifetime, so it must
    // be a singleton. Built via a factory: its ctor's optional TimeSpan?/IScheduler? params can't
    // be resolved by the container, and we want the default 1-second poll cadence.
    containerRegistry.RegisterSingleton<StorageMonitor>(cp => new StorageMonitor(
        cp.Resolve<StorageInfoBuilder>(), cp.Resolve<StorageLoadSource>()));
    containerRegistry.RegisterSingleton<IStorageModel, StorageModel>();

    containerRegistry.Register<IStorageViewModel, StorageViewModel>();

    containerRegistry.RegisterForNavigation<StorageDetailView>(DetailViewNames.Storage);

    ViewModelLocationProvider.Register<StorageSummaryView>(
        () => ContainerLocator.Container.Resolve<IStorageViewModel>());
    ViewModelLocationProvider.Register<StorageDetailView>(
        () => ContainerLocator.Container.Resolve<IStorageViewModel>());
  }

  public void OnInitialized(IContainerProvider containerProvider) {
    // Self-warming loading tile: spinner now, warm the model singleton off the UI thread, swap in
    // the real view when ready. See CpuModule for the rationale.
    _regionManager.RegisterViewWithRegion(RegionNames.StorageRegionName, () => {
      var host = new LoadingHost { Label = "Storage" };
      host.Begin(
          () => containerProvider.Resolve<IStorageModel>(),
          () => new StorageSummaryView());
      return host;
    });
  }
}
