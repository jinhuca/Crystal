using System;
using Crystal;

namespace Crystal.Wpf.Tests.Mocks.Modules
{
	[Module(ModuleName = "TestModule", OnDemand = true)]
	public class MockAttributedModule : IModule
	{
		public void OnInitialized(IContainerProvider containerProvider)
		{
			throw new NotImplementedException();
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
			throw new NotImplementedException();
		}
	}
}
