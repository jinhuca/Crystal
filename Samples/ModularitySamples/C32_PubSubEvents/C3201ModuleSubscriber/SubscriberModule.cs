using C3201ModuleSubscriber.Views;
using C3201SolutionCore;
using Crystal;
using System;

namespace C3201Subscriber
{
  public class SubscriberModule : IModule
  {
    private readonly IRegionManager _regionManager;
    private readonly IContainerProvider _containerProvider;

    public SubscriberModule(IContainerProvider containerProvider, IRegionManager regionManager)
    {
      _containerProvider = containerProvider;
      _regionManager = regionManager;
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
      _regionManager.RegisterViewWithRegion(RegionNames.RightRegion, typeof(SubscriberView));
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
  }
}
