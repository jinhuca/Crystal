using Crystal;
using RegionDemoInfra;

namespace ModuleA
{
	[Module(ModuleName = "Module A", OnDemand = true)]
	public class ModuleAModule : IModule
	{
		readonly IContainerProvider _container;
		readonly IRegionManager _regionManager;

		public ModuleAModule(IContainerProvider containerProvider, IRegionManager regionManager)
		{
			_container = containerProvider;
			_regionManager = regionManager;
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			//IRegion region = _regionManager.Regions[RegionNames.ToolbarRegion];
			//region.Add(_container.Resolve<ToolbarView>());

			_regionManager.RegisterViewWithRegion(RegionNames.ToolbarRegion, typeof(ToolbarView));
			_regionManager.RegisterViewWithRegion(RegionNames.ContentRegion, typeof(ContentView));
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}
	}
}
