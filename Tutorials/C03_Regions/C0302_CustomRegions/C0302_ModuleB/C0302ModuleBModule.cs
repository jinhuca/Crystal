using C0302_RegionInfra;
using Crystal;

namespace C0302_ModuleB
{
	public class C0302ModuleBModule : IModule
	{
		private readonly IContainerProvider _containerProvider;
		private readonly IRegionManager _regionManager;

		public C0302ModuleBModule(IContainerProvider containerProvider, IRegionManager regionManager)
		{
			_containerProvider = containerProvider;
			_regionManager = regionManager;
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			_regionManager.RegisterViewWithRegion(RegionNames.ContentRegion, typeof(ContentView));
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}
	}
}
