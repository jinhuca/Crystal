using Crystal.Infrastructure.Constants;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.Provider.Mmi.MmiEngine;
using MemoryModule.Models;
using MemoryModule.ViewModels;
using MemoryModule.Views;

namespace MemoryModule;

/// <summary>
/// Prism module for Memory (static inventory only). Registers the provider→builder→model→
/// view-model chain, injects the compact <see cref="MemorySummaryView"/> into the dashboard's
/// memory tile region, and registers the full-scale <see cref="MemoryDetailView"/> for navigation.
/// </summary>
public class MemoryModule(IRegionManager regionManager) : IModule {
  private readonly IRegionManager _regionManager = regionManager;

  public void RegisterTypes(IContainerRegistry containerRegistry) {
    containerRegistry.Register<IWmiHardwareProvider, WmiHardwareProvider>();
    containerRegistry.Register<MemoryInfoBuilder>();

    // MemoryLoadSource owns an open LibreHardwareMonitor Computer; keep one for the app lifetime.
    containerRegistry.RegisterSingleton<MemoryLoadSource>();

    // MemoryModel replays its one-shot spec build and owns the load-polling lifetime, so it must
    // be a singleton. Built via a factory: its ctor's optional TimeSpan?/IScheduler? params can't
    // be resolved by the container, and we want the default 1-second poll cadence.
    containerRegistry.RegisterSingleton<IMemoryModel>(cp => new MemoryModel(
        cp.Resolve<MemoryInfoBuilder>(), cp.Resolve<MemoryLoadSource>()));

    containerRegistry.Register<IMemoryViewModel, MemoryViewModel>();

    containerRegistry.RegisterForNavigation<MemoryDetailView>(DetailViewNames.Memory);

    ViewModelLocationProvider.Register<MemorySummaryView>(
        () => ContainerLocator.Container.Resolve<IMemoryViewModel>());
    ViewModelLocationProvider.Register<MemoryDetailView>(
        () => ContainerLocator.Container.Resolve<IMemoryViewModel>());
  }

  public void OnInitialized(IContainerProvider containerProvider) {
    _regionManager.RegisterViewWithRegion(RegionNames.MemoryRegionName, typeof(MemorySummaryView));
  }
}
