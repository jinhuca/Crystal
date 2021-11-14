using Crystal;
using ModuleA.Views;

namespace ModuleA
{
  public class ModuleAModule : IModule
  {
    private readonly IRegionManager _regionManager;
    public ModuleAModule(IRegionManager regionManager)
    {
      _regionManager = regionManager;
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
      _regionManager.RegisterViewWithRegion("ContentRegion", typeof(PersonList));
      _regionManager.RegisterViewWithRegion("PersonDetailsRegion", typeof(PersonDetail));
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
  }
}
