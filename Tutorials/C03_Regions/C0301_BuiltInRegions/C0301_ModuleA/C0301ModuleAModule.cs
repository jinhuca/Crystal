using C0301_ModuleA.Views;
using C0301_RegionInfra;
using Crystal;

namespace C0301_ModuleA
{
	[Module(ModuleName = "Module A", OnDemand = true)]
	public class C0301ModuleAModule : IModule
	{
		private readonly IContainerProvider _containerProvider;
		private readonly IRegionManager _regionManager;

		public C0301ModuleAModule(IContainerProvider containerProvider, IRegionManager regionManager)
		{
			_containerProvider = containerProvider;
			_regionManager = regionManager;
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			_regionManager.RegisterViewWithRegion(RegionNames.ToolbarRegion, typeof(ToolbarView));
			_regionManager.RegisterViewWithRegion(RegionNames.ContentRegion, typeof(ContentView));
		}
	}
}
