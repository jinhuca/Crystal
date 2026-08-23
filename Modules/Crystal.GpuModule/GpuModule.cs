using Crystal.Controls.Loading;
using Crystal.GpuModule.Models;
using Crystal.GpuModule.ViewModels;
using Crystal.GpuModule.Views;
using Crystal.Infrastructure.Constants;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Service.Gpu;

namespace Crystal.GpuModule;

/// <summary>
/// Prism module for GPU. Registers the provider→builder→monitor→model→view-model chain, injects
/// the compact <see cref="GpuSummaryView"/> into the dashboard's GPU tile region, and registers
/// the full-scale <see cref="GpuDetailView"/> for navigation.
/// </summary>
public class GpuModule(IRegionManager regionManager) : IModule {
  /// <summary>
  /// The <see cref="IRegionManager"/> is used to inject the GPU summary view into the dashboard's
  /// GPU tile region.
  /// </summary>
  private readonly IRegionManager _regionManager = regionManager;

  /// <summary>
  /// Registers the GPU module's types with the Prism container.
  /// </summary>
  /// <param name="containerRegistry">The container registry.</param>
  public void RegisterTypes(IContainerRegistry containerRegistry) {
    containerRegistry.Register<IWmiHardwareProvider, WmiHardwareProvider>();

    // GpuLoadSource owns an open LibreHardwareMonitor Computer; keep one for the app lifetime.
    // Registered behind IGpuLoadSource so GpuInfoBuilder can be unit-tested against a fake.
    containerRegistry.RegisterSingleton<IGpuLoadSource, GpuLoadSource>();

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

    ViewModelLocationProvider.Register<GpuSummaryView>(() => ContainerLocator.Container.Resolve<IGpuViewModel>());
    ViewModelLocationProvider.Register<GpuDetailView>(() => ContainerLocator.Container.Resolve<IGpuViewModel>());
  }

  /// <summary>
  /// Injects the GPU summary view into the dashboard's GPU tile region, using a loading host to 
  /// warm the model singleton off the UI thread before swapping in the real view.
  /// </summary>
  /// <param name="containerProvider">The container provider.</param>
  public void OnInitialized(IContainerProvider containerProvider) {
    // Self-warming loading tile: spinner now, warm the model singleton off the UI thread, swap in
    // the real view when ready. See CpuModule for the rationale.
    _regionManager.RegisterViewWithRegion(RegionNames.GpuSummaryRegionName, () => {
      var host = new LoadingHost { Label = "GPU" };
      host.Begin(
        () => containerProvider.Resolve<IGpuModel>(),
        () => new GpuSummaryView());
      return host;
    });
  }
}
