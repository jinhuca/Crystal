using C0102ModuleA.Views;
using Crystal;
using System;

namespace C0102Module
{
	public class Module : IModule
	{
		private readonly IContainerProvider _containerProvider;
		private readonly IRegionManager _regionManager;

		public Module(IContainerProvider containerProvider, IRegionManager regionManager)
		{
			_containerProvider = containerProvider;
			_regionManager = regionManager;
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			_regionManager.RegisterViewWithRegion("ContentRegion", typeof(ViewA1));
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}
	}
}
