using C0302RegionInfra;
using Crystal;

namespace C0302ModuleA
{
	[Module(ModuleName = "Module A", OnDemand = true)]
	public class ModuleAModule : IModule
	{
		private readonly IContainerProvider _container;
		private readonly IRegionManager _regionManager;

		public ModuleAModule(IContainerProvider containerProvider, IRegionManager regionManager)
		{
			_container = containerProvider;
			_regionManager = regionManager;
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			_regionManager.RegisterViewWithRegion(RegionNames.ToolbarRegion, typeof(ToolbarView));
			_regionManager.RegisterViewWithRegion(RegionNames.ToolbarRegion, typeof(ToolbarView));
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}
	}
}
