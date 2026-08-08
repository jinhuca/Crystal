using BiosModule.Models;
using BiosModule.ViewModels;
using BiosModule.Views;
using Crystal.Controls.Loading;
using Crystal.Infrastructure.Constants;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.Provider.Mmi.MmiEngine;

namespace BiosModule;

/// <summary>
/// Prism module for BIOS (static identity only). Registers the provider→builder→model→
/// view-model chain, injects the full-width <see cref="BiosSummaryView"/> into the dashboard's
/// BIOS region, and registers the full-scale <see cref="BiosDetailView"/> for navigation.
/// </summary>
public class BiosModule(IRegionManager regionManager) : IModule {
  private readonly IRegionManager _regionManager = regionManager;

  public void RegisterTypes(IContainerRegistry containerRegistry) {
    containerRegistry.Register<IWmiHardwareProvider, WmiHardwareProvider>();
    containerRegistry.Register<BiosInfoBuilder>();

    containerRegistry.RegisterSingleton<IBiosModel, BiosModel>();

    containerRegistry.Register<IBiosViewModel, BiosViewModel>();

    containerRegistry.RegisterForNavigation<BiosDetailView>(DetailViewNames.Bios);

    ViewModelLocationProvider.Register<BiosSummaryView>(
        () => ContainerLocator.Container.Resolve<IBiosViewModel>());
    ViewModelLocationProvider.Register<BiosDetailView>(
        () => ContainerLocator.Container.Resolve<IBiosViewModel>());
  }

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
