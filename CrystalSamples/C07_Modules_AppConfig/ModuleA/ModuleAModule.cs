using Crystal.Ioc;
using Crystal.Modularity;
using Crystal.Regions;
using ModuleA.Views;

namespace ModuleA
{
	public class ModuleAModule : IModule
	{
		public void OnInitialized(IContainerProvider containerProvider)
		{
			var regionManager = containerProvider.Resolve<IRegionManager>();
			regionManager.RegisterViewWithRegion("ContentRegion", typeof(ViewA));
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}
	}
}
