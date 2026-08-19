using Crystal.BiosModule.Models;
using Crystal.BiosModule.ViewModels;
using Crystal.BiosModule.Views;
using Crystal.Controls.Loading;
using Crystal.Infrastructure.Constants;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.Provider.Mmi.HardwareFeatures.FirmwareSecurity;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Smbios.HardwareFeatures.Firmware;
using Crystal.Service.Bios;
using Crystal.Service.Sensors;

namespace Crystal.BiosModule;

/// <summary>
/// Prism module for BIOS (static identity only). Registers the provider→builder→model→
/// view-model chain, injects the full-width <see cref="BiosSummaryView"/> into the dashboard's
/// BIOS region, and registers the full-scale <see cref="BiosDetailView"/> for navigation.
/// </summary>
public class BiosModule(IRegionManager regionManager) : IModule {
  /// <summary>
  /// The Prism region manager, used to inject the summary view into the dashboard's BIOS region.
  /// </summary>
  private readonly IRegionManager _regionManager = regionManager;

  /// <summary>
  /// Registers the provider→builder→model→view-model chain, injects the full-width
  /// <see cref="BiosSummaryView"/> into the dashboard's BIOS region.
  /// </summary>
  /// <param name="containerRegistry">The container registry.</param>
  public void RegisterTypes(IContainerRegistry containerRegistry) {
    containerRegistry.Register<IWmiHardwareProvider, WmiHardwareProvider>();
    containerRegistry.Register<ISmbiosFirmwareProvider, SmbiosFirmwareProvider>();
    containerRegistry.Register<IFirmwareSecurityProvider, FirmwareSecurityProvider>();
    containerRegistry.Register<FirmwareInfoBuilder>();
    containerRegistry.RegisterSingleton<BiosMonitor>();

    // Live board telemetry, projected from the shell's shared SensorMonitor (there is no separate
    // Motherboard module; the BIOS tile is the board's home). Optional ctor param → factory lambda.
    containerRegistry.RegisterSingleton<BoardSensorMonitor>(cp => new BoardSensorMonitor(cp.Resolve<SensorMonitor>()));
    containerRegistry.RegisterSingleton<IBiosModel, BiosModel>();
    containerRegistry.Register<IBiosViewModel, BiosViewModel>();
    containerRegistry.RegisterForNavigation<BiosDetailView>(DetailViewNames.Bios);

    ViewModelLocationProvider.Register<BiosSummaryView>(
      () => ContainerLocator.Container.Resolve<IBiosViewModel>());
    ViewModelLocationProvider.Register<BiosDetailView>(
      () => ContainerLocator.Container.Resolve<IBiosViewModel>());
  }

  /// <summary>
  /// Injects the full-width <see cref="BiosSummaryView"/> into the dashboard's BIOS region, 
  /// using a <see cref="LoadingHost"/> to warm the model singleton off the UI thread before swapping in the real view.
  /// </summary>
  /// <param name="containerProvider">The container provider.</param>
  public void OnInitialized(IContainerProvider containerProvider) {
    // Self-warming loading tile: spinner now, warm the model singleton off the UI thread, swap in
    // the real view when ready. See CpuModule for the rationale.
    _regionManager.RegisterViewWithRegion(RegionNames.BiosRegionName, () => {
      var host = new LoadingHost { Label = "BIOS" };
      host.Begin(
        () => containerProvider.Resolve<IBiosModel>(),
        () => new BiosSummaryView());
      return host;
    });
  }
}
