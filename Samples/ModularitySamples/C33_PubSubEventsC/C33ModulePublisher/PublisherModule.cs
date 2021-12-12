using C33ModuleCore;
using C33ModulePublisher.Views;
using Crystal;

namespace ModulePublisher
{
  public class PublisherModule : IModule
  {
    private readonly IRegionManager _regionManager;
    private readonly IContainerProvider _containerProvider;

    public PublisherModule(IContainerProvider containerProvider, IRegionManager regionManager)
    {
      _containerProvider = containerProvider;
      _regionManager = regionManager;
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
      _regionManager.RegisterViewWithRegion(RegionNames.LeftRegionName, typeof(PublisherView));
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
  }
}
