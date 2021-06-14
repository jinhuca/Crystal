using ModuleB.Views;
using Crystal;

namespace ModuleB
{
	public class ModuleBModule : IModule
	{
		public void OnInitialized(IContainerProvider containerProvider)
		{
			var regionManager = containerProvider.Resolve<IRegionManager>();
			regionManager.RegisterViewWithRegion("RightRegion", typeof(MessageList));
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{

		}
	}
}