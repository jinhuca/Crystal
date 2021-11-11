using Crystal;
using ModuleA.Views;
using SharedLibrary;

namespace ModuleA
{
	public class ModuleAModule : IModule
	{
		private readonly IContainer _container;
		private readonly IRegionManager _regionManager;

		public ModuleAModule(IContainer container, IRegionManager regionManager)
		{
			_container = container;
			_regionManager = regionManager;
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{

		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			_regionManager.RegisterViewWithRegion(Regionnames.ToolbarRegionName, typeof(ToolbarA));
			_regionManager.RegisterViewWithRegion(Regionnames.ContentRegionName, typeof(ContentA));
		}
	}
}
