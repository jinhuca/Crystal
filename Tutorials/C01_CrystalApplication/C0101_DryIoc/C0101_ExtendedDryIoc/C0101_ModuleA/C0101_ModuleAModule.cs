using C0101_ModuleA.Views;
using C0101_Shared;
using Crystal;

namespace C0101_ModuleA
{
	public class C0101_ModuleAModule : IModule
	{
		private readonly IContainerProvider _containerProvider;
		private readonly IRegionManager _regionManager;

		public C0101_ModuleAModule(IContainerProvider containerProvider, IRegionManager regionManager)
		{
			_containerProvider = containerProvider;
			_regionManager = regionManager;
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			_regionManager.RegisterViewWithRegion(RegionNames.Region1, typeof(ViewA1));
			_regionManager.RegisterViewWithRegion(RegionNames.Region3, typeof(ViewA2));
		}
	}
}
