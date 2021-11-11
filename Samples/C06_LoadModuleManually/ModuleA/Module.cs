using Crystal;
using ModuleA.Views;
using Shared;

namespace ModuleA
{
	public class Module : IModule
	{
		public void OnInitialized(IContainerProvider containerProvider)
		{
			var regionManager = containerProvider.Resolve<IRegionManager>();
			regionManager.RegisterViewWithRegion(RegionNames.ContentRegionName, typeof(ViewA));
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}
	}
}
