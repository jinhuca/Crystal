using Crystal.Controls.Loading;
using Crystal.CpuModule.Models;
using Crystal.CpuModule.ViewModels.Implementations;
using Crystal.CpuModule.ViewModels.Interfaces;
using Crystal.CpuModule.Views;
using Crystal.Infrastructure.Constants;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.Provider.CpuId;
using Crystal.Provider.Mmi.HardwareFeatures.Processor;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Smbios.HardwareFeatures.Processor;
using Crystal.Service.Cpu;
using Crystal.Service.Sensors;

namespace Crystal.CpuModule;

/// <summary>
/// Prism module for CPU. Registers the provider→service→model→view-model chain, injects the
/// compact <see cref="CpuSummaryView"/> into the dashboard's CPU tile region, and registers
/// the full-scale <see cref="CpuDetailView"/> for navigation.
/// </summary>
public class CpuModule(IRegionManager regionManager) : IModule {
  /// <summary>
  /// The Prism region manager, used to inject the summary view into the dashboard's CPU tile region.
  /// </summary>
  private readonly IRegionManager _regionManager = regionManager;

  /// <summary>
  /// Registers the CPU module's services, models, and view models with the Prism container.
  /// </summary>
  /// <param name="containerRegistry">The container registry.</param>
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

    // CPU fan RPM, projected from the shell's shared SensorMonitor (CPU hardware emits no fan; it
    // comes from the motherboard SuperIO). Built via a factory to inject that singleton.
    containerRegistry.RegisterSingleton<CpuFanMonitor>(cp => new CpuFanMonitor(cp.Resolve<SensorMonitor>()));

    // View models. The sub-view models are per-consumer; the root VM composes them.
    containerRegistry.Register<ICpuSpecsViewModel, CpuSpecsViewModel>();
    containerRegistry.Register<ICpuSensorViewModel, CpuSensorsViewModel>();
    containerRegistry.Register<ICpuViewModel, CpuViewModel>();

    // Register the detail view for region navigation (swapped into the shell's main region).
    containerRegistry.RegisterForNavigation<CpuDetailView>(DetailViewNames.Cpu);

    // Both views set prism:ViewModelLocator.AutoWireViewModel="True". The default convention
    // looks for CpuModule.ViewModels.<ViewName>Model; our VM lives under .Implementations and is
    // resolved by its interface, so map both views to it explicitly. Each view gets its own VM
    // instance (Register, not singleton), so their live graphs never share sample buffers.
    ViewModelLocationProvider.Register<CpuSummaryView>(
      () => ContainerLocator.Container.Resolve<ICpuViewModel>());
    ViewModelLocationProvider.Register<CpuDetailView>(
      () => ContainerLocator.Container.Resolve<ICpuViewModel>());
  }

  /// <summary>
  /// Injects a self-warming loading tile into the dashboard's CPU region: it shows a spinner
  /// </summary>
  /// <param name="containerProvider">The container provider.</param>
  public void OnInitialized(IContainerProvider containerProvider) {
    // Inject a self-warming loading tile into the dashboard's CPU region: it shows a spinner
    // immediately and warms the heavy model singleton (which opens a ring-0 session) on a
    // background thread, swapping in the real summary view when ready. This keeps startup snappy
    // and independent per component, so one slow tile never blocks the rest of the dashboard.
    _regionManager.RegisterViewWithRegion(RegionNames.CpuRegionName, () => {
      var host = new LoadingHost { Label = "CPU" };
      host.Begin(
        () => containerProvider.Resolve<ICpuModel>(),
        () => new CpuSummaryView());
      return host;
    });
  }
}
