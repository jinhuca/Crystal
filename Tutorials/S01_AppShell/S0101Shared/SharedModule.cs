using System;
using Crystal;

namespace S0101Shared
{
	public class SharedModule : IModule
	{
		private readonly IContainerProvider _containerProvider;

		public SharedModule(IContainerProvider containerProvider)
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
