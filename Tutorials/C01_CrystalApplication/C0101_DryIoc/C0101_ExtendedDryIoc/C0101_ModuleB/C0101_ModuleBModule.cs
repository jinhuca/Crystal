using System;
using C0101_ModuleB.Views;
using C0101_Shared;
using Crystal;

namespace C0101_ModuleB
{
	public class C0101_ModuleBModule:IModule
	{
		private readonly IContainerProvider _containerProvider;
		private readonly IRegionManager _regionManager;

		public C0101_ModuleBModule(IContainerProvider containerProvider, IRegionManager regionManager)
		{
			_containerProvider = containerProvider;
			_regionManager = regionManager;
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			_regionManager.RegisterViewWithRegion(RegionNames.Region2, typeof(ViewB1));
		}
	}
}
