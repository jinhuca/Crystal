using Crystal;
using T0202DryIocModule.Views;

namespace T0201DryIocModule
{
	public class T0201DryIocModule : IModule
	{
		private readonly IContainerProvider _containerProvider;
		private readonly IRegionManager _regionManager;

		public T0201DryIocModule(IContainerProvider containerProvider, IRegionManager regionManager)
		{
			_containerProvider = containerProvider;
			_regionManager = regionManager;
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
			
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			_regionManager.RegisterViewWithRegion("ContentRegion", typeof(ViewA));
		}
	}
}
