using Crystal;
using C0100ModuleA.Views;
using C0100Shared;

namespace C0100ModuleA
{
	public class C0100ModuleAModule : IModule
	{
		private readonly IContainerProvider _containerProvider;
		private readonly IRegionManager _regionManager;

		public C0100ModuleAModule(IContainerProvider containerProvider, IRegionManager regionManager)
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
