using BiosModule.Models;
using BiosModule.ViewModels;
using BiosModule.Views;
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
    _regionManager.RegisterViewWithRegion(RegionNames.BiosRegionName, typeof(BiosSummaryView));
  }
}
