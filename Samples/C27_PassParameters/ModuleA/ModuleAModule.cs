using Crystal;
using ModuleA.Views;

namespace ModuleA
{
  public class ModuleAModule : IModule
  {
    public void OnInitialized(IContainerProvider containerProvider)
    {
      var regionManager = containerProvider.Resolve<IRegionManager>();
      regionManager.RegisterViewWithRegion("ContentRegion", typeof(PersonList));
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
      containerRegistry.RegisterForNavigation<PersonDetail>();
    }
  }
}