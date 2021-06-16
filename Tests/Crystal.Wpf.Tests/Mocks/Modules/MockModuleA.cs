using System;
using Crystal;

namespace Crystal.Wpf.Tests.Mocks.Modules
{
	public class MockModuleA : IModule
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

	public class DummyClass
	{
	}
}
