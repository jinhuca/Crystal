using C0505_ModuleA.Views;
using Crystal;

namespace C0505_ModuleA
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
