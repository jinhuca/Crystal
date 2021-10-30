using System;
using Crystal;
using ModuleA.Views;

namespace ModuleA
{
	public class ModuleAModule : IModule
	{
		private readonly IContainerProvider _containerProvider;
		private readonly IRegionManager _regionManager;

		public ModuleAModule(IContainerProvider containerProvider, IRegionManager regionManager)
		{
			_containerProvider = containerProvider;
			_regionManager = regionManager;
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{

		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			_regionManager.RegisterViewWithRegion(Shared.RegionNames.ContentRegionName, typeof(ViewA1));
		}
	}
}
