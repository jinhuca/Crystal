using Crystal;
using ModuleA.Views;

namespace ModuleA
{
  public class ModuleAModule : IModule
  {
    public void OnInitialized(IContainerProvider containerProvider)
    {
      var regionManager = containerProvider.Resolve<IRegionManager>();
      regionManager.RequestNavigate("ContentRegion", "PersonList");
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
      containerRegistry.RegisterForNavigation<PersonList>();
      containerRegistry.RegisterForNavigation<PersonDetail>();
    }
  }
}