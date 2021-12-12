using C3201ModulePublisher.Views;
using C3201SolutionCore;
using Crystal;

namespace C3101Sender
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
      _regionManager.RegisterViewWithRegion(RegionNames.LeftRegion, typeof(PublisherView));
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
      
    }
  }
}
