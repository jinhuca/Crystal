using Crystal;
using ModuleA.Views;
using Shared;

namespace ModuleA
{
	public class Module : IModule
	{
		private readonly IContainerProvider _containerProvider;
		private readonly IRegionManager _regionManager;

		public Module(IContainerProvider containerProvider, IRegionManager regionManager)
		{
			_containerProvider = containerProvider;
			_regionManager = regionManager;
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			_regionManager.RegisterViewWithRegion(RegionNames.ToolbarRegionName, typeof(ViewA1));
			_regionManager.RegisterViewWithRegion(RegionNames.ContentRegionName, typeof(ViewA2));
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}
	}
}
