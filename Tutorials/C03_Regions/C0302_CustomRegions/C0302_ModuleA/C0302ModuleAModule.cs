using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C0302_RegionInfra;
using Crystal;

namespace C0302_ModuleA
{
	[Module(ModuleName = "Module A", OnDemand = true)]
	public class C0302ModuleAModule:IModule
	{
		private readonly IContainerProvider _containerProvider;
		private readonly IRegionManager _regionManager;

		public C0302ModuleAModule(IContainerProvider containerProvider, IRegionManager regionManager)
		{
			_containerProvider = containerProvider;
			_regionManager = regionManager;
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			_regionManager.RegisterViewWithRegion(RegionNames.ToolbarRegion, typeof(ToolbarView));

		}
	}
}
