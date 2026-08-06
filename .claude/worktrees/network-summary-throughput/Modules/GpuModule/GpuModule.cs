using Crystal.Infrastructure.Constants;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.Provider.Mmi.MmiEngine;
using GpuModule.Models;
using GpuModule.ViewModels;
using GpuModule.Views;

namespace GpuModule;

/// <summary>
/// Prism module for GPU. Registers the provider→builder→monitor→model→view-model chain, injects
/// the compact <see cref="GpuSummaryView"/> into the dashboard's GPU tile region, and registers
/// the full-scale <see cref="GpuDetailView"/> for navigation.
/// </summary>
public class GpuModule(IRegionManager regionManager) : IModule {
  private readonly IRegionManager _regionManager = regionManager;

  public void RegisterTypes(IContainerRegistry containerRegistry) {
    containerRegistry.Register<IWmiHardwareProvider, WmiHardwareProvider>();

    // GpuLoadSource owns an open LibreHardwareMonitor Computer; keep one for the app lifetime.
    containerRegistry.RegisterSingleton<GpuLoadSource>();

    // GpuMonitor owns the polling lifetime and its Specs replay cache, so it must be a singleton.
    // Built via a factory: its ctor's optional TimeSpan?/IScheduler? params can't be resolved by
    // the container, and we want the default 1-second poll cadence.
    containerRegistry.Register<GpuInfoBuilder>();
    containerRegistry.RegisterSingleton<GpuMonitor>(cp => new GpuMonitor(cp.Resolve<GpuInfoBuilder>()));
    containerRegistry.RegisterSingleton<IGpuModel, GpuModel>();

    // Each view gets its own VM instance (Register, not singleton) so summary and detail never
    // share adapter graph buffers.
    containerRegistry.Register<IGpuViewModel, GpuViewModel>();

    containerRegistry.RegisterForNavigation<GpuDetailView>(DetailViewNames.Gpu);

    ViewModelLocationProvider.Register<GpuSummaryView>(
        () => ContainerLocator.Container.Resolve<IGpuViewModel>());
    ViewModelLocationProvider.Register<GpuDetailView>(
        () => ContainerLocator.Container.Resolve<IGpuViewModel>());
  }

  public void OnInitialized(IContainerProvider containerProvider) {
    _regionManager.RegisterViewWithRegion(RegionNames.GpuSummaryRegionName, typeof(GpuSummaryView));
  }
}
