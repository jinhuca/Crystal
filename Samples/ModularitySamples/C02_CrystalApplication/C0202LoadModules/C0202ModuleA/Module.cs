using C0202ModuleA.Views;
using Crystal;

namespace C0202ModuleA
{
  public class Module : IModule
  {
    private readonly IRegionManager _regionManager;

    public Module(IContainerProvider containerProvider, IRegionManager regionManager)
    {
      _regionManager = regionManager;
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
      _regionManager.RegisterViewWithRegion("ContentRegion", typeof(ViewA1));
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
  }
}
