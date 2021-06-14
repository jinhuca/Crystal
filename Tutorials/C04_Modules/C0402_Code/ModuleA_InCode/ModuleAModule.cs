using Crystal;
using ModuleA_InCode.Views;
using System;

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
