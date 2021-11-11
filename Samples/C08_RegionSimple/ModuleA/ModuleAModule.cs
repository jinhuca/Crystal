using Crystal;
using ModuleA.Views;
using SharedLibary;

namespace ModuleA
{
	public class ModuleAModule : IModule
	{
		private readonly IContainerProvider _containerProvider;
		private readonly IRegionManager _regionManager;
		private readonly IContainer _container;

		public ModuleAModule(IContainerProvider containerProvider, IRegionManager regionManager, IContainer container)
		{
			_containerProvider = containerProvider;
			_regionManager = regionManager;
			_container = container;
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			IRegion region = _regionManager.Regions[RegionNames.ToolbarRegionName];
			region.Add(_container.Resolve<ToolbarView>());
			region.Add(_container.Resolve<ToolbarView>());
			region.Add(_container.Resolve<ToolbarView>());
			region.Add(_container.Resolve<ToolbarView>());

			_regionManager.RegisterViewWithRegion(RegionNames.ContentRegionName, typeof(ContentView));
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}
	}
}
