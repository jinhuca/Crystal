using C0304_ModuleA.Views;
using Crystal;

namespace C0304_ModuleA
{
	public class ModuleAModule : IModule
	{
		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
			
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			var regionManager = containerProvider.Resolve<IRegionManager>();
			regionManager.RegisterViewWithRegion("ContentRegion", typeof(PersonList));
			regionManager.RegisterViewWithRegion("PersonDetailsRegion", typeof(PersonDetail));
		}
	}
}
