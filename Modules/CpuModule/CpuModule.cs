using CpuModule.Models;
using CpuModule.ViewModels.Implementations;
using CpuModule.ViewModels.Interfaces;
using CpuModule.Views;
using Crystal.Provider.CpuId;
using Crystal.Provider.Mmi.HardwareFeatures.Processor;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Smbios.HardwareFeatures.Processor;
using Crystal.Service.Cpu;

namespace CpuModule;

/// <summary>
/// Prism module for the CPU dashboard. Registers the provider→service→model→view-model
/// chain and injects <see cref="CpuView"/> into the shell's main content region.
/// </summary>
public class CpuModule(IRegionManager regionManager) : IModule {
  /// <summary>Region the shell exposes for module content. Kept public so the shell can
  /// declare the matching <c>prism:RegionManager.RegionName</c>.</summary>
  public const string ContentRegion = "ContentRegion";

  private readonly IRegionManager _regionManager = regionManager;

  public void RegisterTypes(IContainerRegistry containerRegistry) {
    // Hardware providers behind Crystal.Service.Cpu (see CpuInfoBuilder's ctor).
    containerRegistry.Register<ICpuIdProvider, CpuIdProvider>();
    containerRegistry.Register<ISmbiosProcessorProvider, SmbiosProcessorProvider>();
    containerRegistry.Register<IWmiHardwareProvider, WmiHardwareProvider>();
    containerRegistry.Register<ICpuSpecsResolver, CpuSpecsResolver>();
    containerRegistry.RegisterSingleton<ICpuTelemetrySource, TelemetryCpuSensorSource>();

    // Built via a factory: CpuInfoBuilder's telemetry parameter is optional (defaults to null),
    // and Unity won't inject an optional ctor parameter — leaving it null makes every sensor read
    // empty (0.00). Resolve it explicitly so live sensors are wired in.
    containerRegistry.Register<CpuInfoBuilder>(cp => new CpuInfoBuilder(
        cp.Resolve<ICpuIdProvider>(),
        cp.Resolve<ISmbiosProcessorProvider>(),
        cp.Resolve<IWmiHardwareProvider>(),
        cp.Resolve<ICpuSpecsResolver>(),
        cp.Resolve<ICpuTelemetrySource>()));

    // CpuMonitor owns the polling lifetime and its Specs replay cache, so it must be a singleton.
    // Built via a factory: its ctor's optional TimeSpan?/IScheduler? params can't be resolved by
    // the container, and we want the default 1-second poll cadence.
    containerRegistry.RegisterSingleton<CpuMonitor>(cp => new CpuMonitor(cp.Resolve<CpuInfoBuilder>()));
    containerRegistry.RegisterSingleton<ICpuModel, CpuModel>();

    // View models. The sub-view models are per-consumer; the root VM composes them.
    containerRegistry.Register<ICpuSpecsViewModel, CpuSpecsViewModel>();
    containerRegistry.Register<ICpuSensorViewModel, CpuSensorsViewModel>();
    containerRegistry.Register<ICpuViewModel, CpuViewModel>();

    // Register the view for region navigation.
    containerRegistry.RegisterForNavigation<CpuView>();

    // CpuView sets prism:ViewModelLocator.AutoWireViewModel="True". The default convention
    // looks for CpuModule.ViewModels.CpuViewModel; our VM lives under .Implementations and is
    // resolved by its interface, so map it explicitly. The factory resolves through the
    // container, so the VM's constructor dependencies are injected.
    ViewModelLocationProvider.Register<CpuView>(
        () => ContainerLocator.Container.Resolve<ICpuViewModel>());
  }

  public void OnInitialized(IContainerProvider containerProvider) {
    _regionManager.RegisterViewWithRegion(ContentRegion, typeof(CpuView));
  }
}
