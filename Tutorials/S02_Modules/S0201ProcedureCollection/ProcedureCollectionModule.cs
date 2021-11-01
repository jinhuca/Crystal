using System;
using Crystal;
using S0201ProcedureCollection.Views;

namespace S0201ModuleA
{
	public class ProcedureCollectionModule : IModule
	{
		private readonly IContainerProvider _containerProvider;
		private readonly IRegionManager _regionManager;

		public ProcedureCollectionModule(IContainerProvider containerProvider, IRegionManager regionManager)
		{
			_containerProvider = containerProvider;
			_regionManager = regionManager;
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			_regionManager.RegisterViewWithRegion(S0201SharedLibrary.RegionNames.ListRegionName, typeof(ProcedureListView));
		}
	}
}
