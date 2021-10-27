using Crystal;
using System;
using C0100ModuleB.Views;
using C0100Shared;

namespace C0100ModuleB
{
	public class C0100ModuleBModule : IModule
	{
		private readonly IContainerProvider _containerProvider;
		private readonly IRegionManager _regionManager;

		public C0100ModuleBModule(IContainerProvider containerProvider, IRegionManager regionManager)
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
		}
	}
}
