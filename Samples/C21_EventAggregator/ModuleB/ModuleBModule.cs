using Crystal;
using ModuleB.Views;

namespace ModuleB
{
  public class ModuleBModule : IModule
  {
    public void OnInitialized(IContainerProvider containerProvider)
    {
      var regionManager = containerProvider.Resolve<IRegionManager>();
      regionManager.RegisterViewWithRegion("RightRegion", typeof(MessageList));
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {

    }
  }
}