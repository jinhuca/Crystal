using System;
using Crystal;

namespace ModuleB
{
	public class ModuleBModule : IModule
	{
		private readonly IContainerProvider _containerProvider;

		public ModuleBModule(IContainerProvider containerProvider)
		{
			_containerProvider = containerProvider;
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
		}
	}
}
