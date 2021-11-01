using Crystal;
using S0301ModuleA.Views;
using System;

namespace S0301ModuleA
{
	public class ModuleAModule : IModule
	{
		public void OnInitialized(IContainerProvider containerProvider)
		{
			var regionManager = containerProvider.Resolve<IRegionManager>();
			regionManager.RegisterViewWithRegion("ContentRegion", typeof(PersonList));
			regionManager.RegisterViewWithRegion("PersonDetailsRegion", typeof(PersonDetail));
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}
	}
}
