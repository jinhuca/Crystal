using C0102_ModuleB.Views;
using C0102_Shared;
using Crystal;

namespace C0102_ModuleB
{
	public class C0102_ModuleBModule:IModule
	{
		private readonly IContainerProvider _containerProvider;
		private readonly IRegionManager _regionManager;

		public C0102_ModuleBModule(IContainerProvider containerProvider, IRegionManager regionManager)
		{
			_containerProvider = containerProvider;
			_regionManager = regionManager;
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			_regionManager.RegisterViewWithRegion(RegionNames.Region3, typeof(ViewB1));
			_regionManager.RegisterViewWithRegion(RegionNames.Region4, typeof(ViewB2));
		}
	}
}
