using Crystal;
using S0201ProcedureDetails.Views;

namespace S0201ModuleB
{
	public class ProcedureDetailsModule : IModule
	{
		private readonly IContainerProvider _containerProvider;
		private readonly IRegionManager _regionManager;

		public ProcedureDetailsModule(IContainerProvider containerProvider, IRegionManager regionManager)
		{
			_containerProvider = containerProvider;
			_regionManager = regionManager;
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			_regionManager.RegisterViewWithRegion(S0201SharedLibrary.RegionNames.DetailsRegionName, typeof(DetailsView));
		}
	}
}
