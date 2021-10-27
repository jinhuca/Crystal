using C0102_ModuleA.Views;
using C0102_Shared;
using Crystal;

namespace C0102_ModuleA
{
	public class C0102_ModuleAModule : IModule
	{
		private readonly IContainerProvider _containerProvider;
		private readonly IRegionManager _regionManager;

		public C0102_ModuleAModule(IContainerProvider containerProvider, IRegionManager regionManager)
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
			_regionManager.RegisterViewWithRegion(RegionNames.Region2, typeof(ViewA2));
		}
	}
}
