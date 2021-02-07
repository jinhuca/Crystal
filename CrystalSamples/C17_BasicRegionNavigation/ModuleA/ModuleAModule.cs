using Crystal.Ioc;
using Crystal.Modularity;
using ModuleA.Views;

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