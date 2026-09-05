using Crystal.Controls.Loading;
using Crystal.Infrastructure.Constants;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.MemoryModule.Models;
using Crystal.MemoryModule.ViewModels;
using Crystal.MemoryModule.Views;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Service.Memory;

namespace Crystal.MemoryModule;

/// <summary>
/// Prism module for Memory (static inventory only). Registers the provider→builder→model→
/// view-model chain and injects the <see cref="MemorySummaryView"/> into the dashboard's
/// memory tile region.
/// </summary>
public class MemoryModule(IRegionManager regionManager) : IModule {
  private readonly IRegionManager _regionManager = regionManager;

  public void RegisterTypes(IContainerRegistry containerRegistry) {
    containerRegistry.Register<IWmiHardwareProvider, WmiHardwareProvider>();
    containerRegistry.Register<MemoryInfoBuilder>();

    // MemoryLoadSource owns an open LibreHardwareMonitor Computer; keep one for the app lifetime.
    // Registered behind IMemoryLoadSource so MemoryMonitor can be unit-tested against a fake.
    containerRegistry.RegisterSingleton<IMemoryLoadSource, MemoryLoadSource>();

    // MemoryMonitor replays its one-shot spec build and owns the load-polling lifetime, so it must
    // be a singleton. Built via a factory: its ctor's optional TimeSpan?/IScheduler? params can't
    // be resolved by the container, and we want the default 1-second poll cadence.
    containerRegistry.RegisterSingleton<MemoryMonitor>(cp => new MemoryMonitor(
        cp.Resolve<MemoryInfoBuilder>(), cp.Resolve<IMemoryLoadSource>()));
    containerRegistry.RegisterSingleton<IMemoryModel, MemoryModel>();

    containerRegistry.Register<IMemoryViewModel, MemoryViewModel>();

    ViewModelLocationProvider.Register<MemorySummaryView>(
        () => ContainerLocator.Container.Resolve<IMemoryViewModel>());
  }

  public void OnInitialized(IContainerProvider containerProvider) {
    // Self-warming loading tile: spinner now, warm the model singleton off the UI thread, swap in
    // the real view when ready. See CpuModule for the rationale.
    _regionManager.RegisterViewWithRegion(RegionNames.MemoryRegionName, () => {
      var host = new LoadingHost { Label = "Memory" };
      host.Begin(
          () => containerProvider.Resolve<IMemoryModel>(),
          () => new MemorySummaryView());
      return host;
    });
  }
}
