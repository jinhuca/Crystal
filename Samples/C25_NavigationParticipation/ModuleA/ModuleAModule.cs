using ModuleA.Views;
using Crystal;

namespace ModuleA
{
  public class ModuleAModule : IModule
  {
    public void OnInitialized(IContainerProvider containerProvider)
    {

    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
      containerRegistry.RegisterForNavigation<ViewA>();
      containerRegistry.RegisterForNavigation<ViewB>();
    }
  }
}