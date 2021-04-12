using C0302RegionInfra;
using Crystal;
using System;

namespace C0302ModuleB
{
	[Module(ModuleName = "Module B", OnDemand = true)]
	public class ModuleBModule : IModule
	{
		private readonly IContainerProvider _container;
		private readonly IRegionManager _regionManager;

		public ModuleBModule(IContainerProvider containerProvider, IRegionManager regionManager)
		{
			_container = containerProvider;
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
