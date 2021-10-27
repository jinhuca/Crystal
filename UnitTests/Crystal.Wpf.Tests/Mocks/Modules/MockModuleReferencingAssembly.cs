using System;
using Crystal;

namespace Crystal.Wpf.Tests.Mocks.Modules
{
	public class MockModuleReferencingAssembly : IModule
	{
		public void OnInitialized(IContainerProvider containerProvider)
		{
			MockReferencedModule instance = new MockReferencedModule();
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
			throw new NotImplementedException();
		}
	}
}
