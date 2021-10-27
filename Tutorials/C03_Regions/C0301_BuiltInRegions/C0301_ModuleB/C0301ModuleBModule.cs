using C0301_ModuleB.Views;
using C0301_RegionInfra;
using Crystal;

namespace C0301_ModuleB
{
	public class C0301ModuleBModule : IModule
	{
		private readonly IContainerProvider _containerProvider;
		private readonly IRegionManager _regionManager;

		public C0301ModuleBModule(IContainerProvider containerProvider, IRegionManager regionManager)
		{
			_containerProvider = containerProvider;
			_regionManager = regionManager;
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			_regionManager.RegisterViewWithRegion(RegionNames.ContentRegion1, typeof(ViewB1));
			_regionManager.RegisterViewWithRegion(RegionNames.ContentRegion2, typeof(ViewB2));
		}
	}
}
