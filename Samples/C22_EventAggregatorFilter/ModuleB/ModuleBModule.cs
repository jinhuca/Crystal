using Crystal;
using ModuleB.Views;

namespace ModuleB
{
  public class ModuleBModule : IModule
  {
    private readonly IRegionManager _regionManager;

    public ModuleBModule(IRegionManager regionManager)
    {
      _regionManager = regionManager;
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
      _regionManager.RegisterViewWithRegion("RightRegion", typeof(MessageList));
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
  }
}
